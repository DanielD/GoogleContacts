using System;
using UIKit;
using OpenId.AppAuth;
using Foundation;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Xamarin.Forms;
using GoogleContacts.iOS.Services;

namespace GoogleContacts.iOS.Views
{
	public class GoogleViewController : UIViewController, IAuthStateChangeDelegate, IAuthStateErrorDelegate
	{
		// The authorization state.  This is the AppAuth object that you should keep araound and serialize to disk.
		private AuthState _authState;
		public AuthState AuthState 
		{
			get { return _authState; }
			set 
			{
				if (_authState != value)
				{
					_authState = value;
					if (_authState != null)
					{
						_authState.StateChangeDelegate = this;
					}
					StateChanged();
				}
			}
		}

		public const string kIssuer = @"https://accounts.google.com";
		public const string kClientID = "<Client ID>";
		public const string kRedirectURI = "zone.dhillon.googlecontacts:oauthredirect";
		public static NSString kAppAuthExampleAuthStateKey = (NSString)"authState";
		
		public GoogleViewController() 
		{
			_tokenService = new TokenService();
		}

		TokenService _tokenService;

		UIButton button;

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			LoadState();

			Title = "Contacts";

			NavigationController.NavigationBar.BarTintColor = UIColor.Clear.FromHex("#8491F9");
			View.BackgroundColor = UIColor.White;

			button = new UIButton(new RectangleF(10, 140, 300, 30));
			button.SetTitle("Login", UIControlState.Normal);
			button.SetTitleColor(UIColor.Blue, UIControlState.Normal);
			button.TouchUpInside += (sender, e) => 
			{
				AuthWithAutoCodeExchange((UIButton)sender);
			};

			View.Add(button);
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);

			//AuthWithAutoCodeExchange(null);
		}

		// Authorization code flow using OIDAuthState automatic code exchanges.
		async void AuthWithAutoCodeExchange(UIButton sender)
		{
			var issuer = new NSUrl(kIssuer);
			var redirectURI = new NSUrl(kRedirectURI);

			Console.WriteLine($"Fetching configuration for issuer: {issuer}");

			try
			{
				// discovers endpoints
				var configuration = await AuthorizationService.DiscoverServiceConfigurationForIssuerAsync(issuer);

				Console.WriteLine($"Got configuration: {configuration}");

				// builds authentication request
				var request = new AuthorizationRequest(configuration, kClientID, new string[] { Scope.OpenId, Scope.Profile, "https://www.googleapis.com/auth/contacts.readonly" }, redirectURI, ResponseType.Code, null);
				// performs authentication request
				var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
				Console.WriteLine($"Initiating authorization request with scope: {request.Scope}");

				appDelegate.CurrentAuthorizationFlow = AuthState.PresentAuthorizationRequest(request, this, (authState, error) =>
				{
					if (authState != null)
					{
						AuthState = authState;
						Console.WriteLine($"Got authorization tokens. Access token: {authState.LastTokenResponse.AccessToken}");

						_tokenService.SetToken(new NSString(AuthState.LastTokenResponse.AccessToken));
						_tokenService.SetRefreshToken(new NSString(AuthState.LastTokenResponse.RefreshToken));

						var contactsViewController = App.GetContactsPage().CreateViewController();
						NavigationController.PresentViewController(contactsViewController, true, null);
					}
					else {
						Console.WriteLine($"Authorization error: {error.LocalizedDescription}");
						AuthState = null;
					}
				});
			}
			catch (Exception ex)
			{

				Console.WriteLine($"Error retrieving discovery document: {ex}");
				AuthState = null;
			}
		}

		// Authorization code flow without a the code exchange (need to call codeExchange manually)
		async void AuthNoCodeExchange(UIButton sender)
		{
			var issuer = new NSUrl(kIssuer);
			var redirectURI = new NSUrl(kRedirectURI);

			Console.WriteLine($"Fetching configuration for issuer: {issuer}");

			try
			{
				// discovers endpoints
				var configuration = await AuthorizationService.DiscoverServiceConfigurationForIssuerAsync(issuer);

				Console.WriteLine($"Got configuration: {configuration}");

				// builds authentication request
				AuthorizationRequest request = new AuthorizationRequest(configuration, kClientID, new string[] { Scope.OpenId, Scope.Profile }, redirectURI, ResponseType.Code, null);
				// performs authentication request
				var appDelegate = (AppDelegate)UIApplication.SharedApplication.Delegate;
				Console.WriteLine($"Initiating authorization request: {request}");
				appDelegate.CurrentAuthorizationFlow = AuthorizationService.PresentAuthorizationRequest(request, this, (authorizationResponse, error) =>
				{
					if (authorizationResponse != null)
					{
						AuthState authState = new AuthState(authorizationResponse);
						AuthState = authState;

						Console.WriteLine($"Authorization response with code: {authorizationResponse.AuthorizationCode}");
						// could just call [self tokenExchange:nil] directly, but will let the user initiate it.
					}
					else
					{
						Console.WriteLine($"Authorization error: {error.LocalizedDescription }");
					}
				});
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error retrieving discovery document: {ex}");
				AuthState = null;
			}
		}

		// Performs the authorization code exchange at the token endpoint.
		async void CodeExchange(UIButton sender)
		{
			// performs code exchange request
			TokenRequest tokenExchangeRequest = AuthState.LastAuthorizationResponse.CreateTokenExchangeRequest();

			Console.WriteLine($"Performing authorization code exchange with request: {tokenExchangeRequest}");

			try
			{
				var tokenResponse = await AuthorizationService.PerformTokenRequestAsync(tokenExchangeRequest);
				Console.WriteLine($"Received token response with accessToken: {tokenResponse.AccessToken}");

				AuthState.Update(tokenResponse, null);
			}
			catch (NSErrorException ex)
			{
				AuthState.Update(ex.Error);

				Console.WriteLine($"Token exchange error: {ex}");
				AuthState = null;
			}
		}

		// Performs a Userinfo API call using OIDAuthState.withFreshTokensPerformAction.
		void Userinfo(UIButton sender)
		{
			var userinfoEndpoint = AuthState.LastAuthorizationResponse.Request.Configuration.DiscoveryDocument.UserinfoEndpoint;
			if (userinfoEndpoint == null)
			{
				Console.WriteLine($"Userinfo endpoint not declared in discovery document");
				return;
			}

			var currentAccessToken = AuthState.LastTokenResponse.AccessToken;

			Console.WriteLine($"Performing userinfo request");

			AuthState.PerformWithFreshTokens(async (accessToken, idToken, error) =>
			{
				if (error != null)
				{
					Console.WriteLine($"Error fetching fresh tokens: {error.LocalizedDescription}");
					return;
				}

				// log whether a token refresh occurred
				if (currentAccessToken != accessToken)
				{
					Console.WriteLine($"Access token was refreshed automatically ({currentAccessToken} to {accessToken})");
				}
				else {
					Console.WriteLine($"Access token was fresh and not updated {accessToken}");
				}

				// creates request to the userinfo endpoint, with access token in the Authorization header
				var httpClient = new HttpClient();
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

				// performs HTTP request
				var response = await httpClient.GetAsync(userinfoEndpoint);
				var content = await response.Content.ReadAsStringAsync();
				NSError deserializeError;
				var data = (NSDictionary)NSJsonSerialization.Deserialize(NSData.FromString(content), 0, out deserializeError);

				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine($"Success: {content}");

					new UIAlertView("OpenID AppAuth", $"Hello, {data["name"]}!", null, "Hi").Show();
				}
				else
				{
					// server replied with an error

					if (response.StatusCode == HttpStatusCode.Unauthorized)
					{
						// "401 Unauthorized" generally indicates there is an issue with the authorization
						// grant. Puts OIDAuthState into an error state.
						var authError = ErrorUtilities.CreateResourceServerAuthorizationError(0, data, error);
						AuthState.Update(authError);
						// log error
						Console.WriteLine($"Authorization Error ({authError}). Response: {content}");
					}
					else
					{
						// log error
						Console.WriteLine($"HTTP Error ({response.StatusCode}). Response: {content}");
					}
				}
			});
		}

		// Nils the OIDAuthState object.
		void ClearAuthState(UIButton sender)
		{
			AuthState = null;
		}

		// Saves the OIDAuthState to NSUSerDefaults.
		private void SaveState()
		{
			// for production usage consider using the OS Keychain instead
			if (AuthState != null)
			{
				var archivedAuthState = NSKeyedArchiver.ArchivedDataWithRootObject(AuthState);
				NSUserDefaults.StandardUserDefaults[kAppAuthExampleAuthStateKey] = archivedAuthState;
			}
			else
			{
				NSUserDefaults.StandardUserDefaults.RemoveObject(kAppAuthExampleAuthStateKey);
			}
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		// Loads the OIDAuthState from NSUSerDefaults.
		private void LoadState()
		{
			// loads OIDAuthState from NSUSerDefaults
			var archivedAuthState = (NSData)NSUserDefaults.StandardUserDefaults[kAppAuthExampleAuthStateKey];
			if (archivedAuthState != null)
			{
				AuthState = (AuthState)NSKeyedUnarchiver.UnarchiveObject(archivedAuthState);
			}
		}

		private void StateChanged()
		{
			SaveState();
		}

		void IAuthStateChangeDelegate.DidChangeState(AuthState state)
		{
			StateChanged();
		}

		void IAuthStateErrorDelegate.DidEncounterAuthorizationError(AuthState state, NSError error)
		{
			Console.WriteLine($"Received authorization error: {error}");
		}
	}
}
