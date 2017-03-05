using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using OpenId.AppAuth;
using Org.Json;
using Xamarin.Forms.Platform.Android;

namespace GoogleContacts.Droid
{
	[Activity(Label = "@string/app_name_short", MainLauncher = true, Theme = "@style/MyTheme", WindowSoftInputMode = SoftInput.StateHidden)]
	public class TokenActivity : FormsApplicationActivity
	{
		#region Members

		static string KEY_AUTH_STATE = "authState";
		static string KEY_USER_INFO = "userInfo";

		static string EXTRA_AUTH_SERVICE_DISCOVERY = "authServiceDiscovery";
		static string EXTRA_AUTH_STATE = "authState";

		AuthState _authState;
		AuthorizationService _authService;
		JSONObject _userInfoJson;

		#endregion

		#region Methods

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			//SetContentView(Resource.Layout.activity_token);

			_authService = new AuthorizationService(this);

			if (savedInstanceState != null)
			{
				if (savedInstanceState.ContainsKey(KEY_AUTH_STATE))
				{
					try
					{
						_authState = AuthState.JsonDeserialize(savedInstanceState.GetString(KEY_AUTH_STATE));
					}
					catch (Exception ex)
					{
						Console.WriteLine("Malformed authorization JSON saved: " + ex);
					}
				}

				if (savedInstanceState.ContainsKey(KEY_USER_INFO))
				{
					try
					{
						_userInfoJson = new JSONObject(savedInstanceState.GetString(KEY_USER_INFO));
					}
					catch (Exception ex)
					{
						Console.WriteLine("Failed to parse saved user info JSON: " + ex);
					}
				}
			}

			if (_authState == null)
			{
				_authState = GetAuthStateFromIntent(Intent);
				var response = AuthorizationResponse.FromIntent(Intent);
				var ex = AuthorizationException.FromIntent(Intent);
				_authState.Update(response, ex);

				if (response != null)
				{
					Console.WriteLine("Received AuthorizationResponse");
					PerformTokenRequest(response.CreateTokenExchangeRequest());
				}
				else 
				{
					Console.WriteLine("Authorization failed: " + ex);
				}
			}

			//RefreshUi();

			//var contactsViewController = App.GetContactsPage().CreateViewController();
			//NavigationController.PresentViewController(contactsViewController, true, null);
			Xamarin.Forms.Forms.Init(this, savedInstanceState);
			LoadApplication(new App());
		}

		protected override void OnSaveInstanceState(Bundle outState)
		{
			if (_authState != null)
				outState.PutString(KEY_AUTH_STATE, _authState.JsonSerializeString());

			if (_userInfoJson != null)
				outState.PutString(KEY_USER_INFO, _userInfoJson.ToString());
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_authService.Dispose();
		}

		void ReceivedTokenResponse(TokenResponse tokenResponse, AuthorizationException authException)
		{
			Console.WriteLine("Token request complete");
			_authState.Update(tokenResponse, authException);
			RefreshUi();
		}

		void RefreshUi()
		{
			//
		}

		void PerformTokenRequest(TokenRequest tokenRequest)
		{
			IClientAuthentication clientAuthentication;
			try
			{
				clientAuthentication = _authState.ClientAuthentication;
			}
			catch (ClientAuthenticationUnsupportedAuthenticationMethod ex)
			{
				Console.WriteLine("Token request cannot be made, client authorization for the token endpoint could not be constructed: " + ex);
				return;
			}

			_authService.PerformTokenRequest(tokenRequest, ReceivedTokenResponse);
		}

		void FetchUserInfo()
		{
			if (_authState.AuthorizationServiceConfiguration == null)
				Console.WriteLine("Cannot make userInfo request without service configuration.");

			_authState.PerformActionWithFreshTokens(_authService, async (accessToken, idToken, ex) =>
			{
				if (ex != null)
				{
					Console.WriteLine("Token refresh failed when fetching user info.");
					return;
				}

				var discoveryDoc = GetDiscoveryDocFromIntent(Intent);
				if (discoveryDoc == null)
				{
					throw new InvalidOperationException("No available discovery doc.");
				}

				try
				{
					using (var client = new HttpClient())
					{
						client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
						var response = await client.GetStringAsync(discoveryDoc.UserinfoEndpoint.ToString());

						new Handler(MainLooper).Post(() =>
						{
							_userInfoJson = new JSONObject(response);
							RefreshUi();
						});
					}
				}
				catch (HttpRequestException ioEx)
				{
					Console.WriteLine("Network error when quering userinfo endpoint: " + ioEx);
				}
				catch (JSONException jsonEx)
				{
					Console.WriteLine("Failed to parse userinfo response: " + jsonEx);
				}
			});
		}

		public static PendingIntent CreatePostAuthorizationIntent(Context context, AuthorizationRequest authRequest, AuthorizationServiceDiscovery discoveryDoc, AuthState authState)
		{
			var intent = new Intent(context, typeof(TokenActivity));
			intent.PutExtra(EXTRA_AUTH_STATE, authState.JsonSerializeString());
			if (discoveryDoc != null)
				intent.PutExtra(EXTRA_AUTH_SERVICE_DISCOVERY, discoveryDoc.DocJson.ToString());

			return PendingIntent.GetActivity(context, authRequest.GetHashCode(), intent, 0);
		}

		static AuthorizationServiceDiscovery GetDiscoveryDocFromIntent(Intent intent)
		{
			if (!intent.HasExtra(EXTRA_AUTH_SERVICE_DISCOVERY))
				return null;

			var discoveryJson = intent.GetStringExtra(EXTRA_AUTH_SERVICE_DISCOVERY);
			try
			{
				return new AuthorizationServiceDiscovery(new JSONObject(discoveryJson));
			}
			catch (JSONException ex)
			{
				throw new InvalidOperationException("Malformed JSON in discovery doc", ex);
			}
			catch (AuthorizationServiceDiscovery.MissingArgumentException ex)
			{
				throw new InvalidOperationException("Malformed JSON in discovery doc", ex);
			}
		}

		static AuthState GetAuthStateFromIntent(Intent intent)
		{
			if (!intent.HasExtra(EXTRA_AUTH_STATE))
				throw new InvalidOperationException("The AuthState instance is missing in the intent.");

			try
			{
				return AuthState.JsonDeserialize(intent.GetStringExtra(EXTRA_AUTH_STATE));
			}
			catch (JSONException ex)
			{
				Console.WriteLine("Malformed AuthState JSON saved: " + ex);
				throw new InvalidOperationException("The AuthState instance is missing in the intent.", ex);
			}
		}

		#endregion
	}
}
