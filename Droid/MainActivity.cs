using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using OpenId.AppAuth;
using Xamarin.Forms.Platform.Android;

namespace GoogleContacts.Droid
{
	[Activity(Label = "@string/app_name_short", Icon = "@drawable/icon", Theme = "@style/MyTheme", MainLauncher = true, WindowSoftInputMode = SoftInput.StateHidden)]
	public class MainActivity : FormsApplicationActivity
	{
		#region Members

		static string DiscoveryEndpoint = "https://accounts.google.com/.well-known/openid-configuration";
		static string ClientId = "<Client DI>";
		static string RedirectUrl = "zone.dhillon.googlecontacts:/oauthredirect";

		static string AuthEndpoint = null;
		static string TokenEndpoint = null;
		static string RegistrationEndpoint = null;

		Button _button;
		AuthorizationService _authService;

		#endregion

		#region Methods

		protected override void OnCreate(Bundle bundle)
		{
			base.OnCreate(bundle);

			SetContentView(Resource.Layout.activity_main);

			_button = FindViewById<Button>(Resource.Id.idpButton);
			_button.Click += async delegate
			{
				Console.WriteLine("initiating auth...");

				try
				{
					AuthorizationServiceConfiguration serviceConfiguration;
					if (DiscoveryEndpoint != null)
					{
						serviceConfiguration = await AuthorizationServiceConfiguration.FetchFromUrlAsync(
							Android.Net.Uri.Parse(DiscoveryEndpoint));
					}
					else 
					{
						serviceConfiguration = new AuthorizationServiceConfiguration(
							Android.Net.Uri.Parse(AuthEndpoint), 
							Android.Net.Uri.Parse(TokenEndpoint), 
							Android.Net.Uri.Parse(RegistrationEndpoint));
					}

					Console.WriteLine("configuration retrieved, proceeding");
					if (ClientId == null)
					{
						// do dynamic client registration if no client_id
						MakeRegistrationRequest(serviceConfiguration);
					}
					else 
					{
						MakeAuthRequest(serviceConfiguration, new AuthState());
					}
				}
				catch (AuthorizationException ex)
				{
					Console.WriteLine("Failed to retrieve configuration: " + ex);
				}
			};

			global::Xamarin.Forms.Forms.Init(this, bundle);
			LoadApplication(new App());
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			_authService.Dispose();
		}

		void MakeAuthRequest(AuthorizationServiceConfiguration serviceConfig, AuthState authState)
		{
			var authRequest = new AuthorizationRequest.Builder(serviceConfig, ClientId, ResponseTypeValues.Code, Android.Net.Uri.Parse(RedirectUrl))
			                                          .SetScope("openid profile email https://www.googleapis.com/auth/contacts.readonly")
			                                          .Build();

			Console.WriteLine("Making auth request to " + serviceConfig.AuthorizationEndpoint);
			_authService.PerformAuthorizationRequest(
				authRequest, 
				TokenActivity.CreatePostAuthorizationIntent(this, authRequest, serviceConfig.DiscoveryDoc, authState), 
				_authService.CreateCustomTabsIntentBuilder().SetToolbarColor(Resource.Color.colorAccent).Build());
		}

		async void MakeRegistrationRequest(AuthorizationServiceConfiguration serviceConfig)
		{
			var registrationRequest = new RegistrationRequest.Builder(serviceConfig, new[] { Android.Net.Uri.Parse(RedirectUrl) })
			                                                 .SetTokenEndpointAuthenticationMethod(ClientSecretBasic.Name)
			                                                 .Build();

			Console.WriteLine("Making registration request to " + serviceConfig.RegistrationEndpoint);

			try
			{
				var registrationResponse = await _authService.PerformRegistrationRequestAsync(registrationRequest);
				Console.WriteLine("Registration request complete");

				if (registrationResponse != null)
				{
					ClientId = registrationResponse.ClientId;
					Console.WriteLine("Registration request completed successfully");
					// continue with authentication
					MakeAuthRequest(registrationResponse.Request.Configuration, new AuthState(registrationResponse));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("Registration request had an error: " + ex);
			}
		}

		#endregion
	}
}
