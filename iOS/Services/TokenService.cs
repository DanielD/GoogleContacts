using System;
using Foundation;
using GoogleContacts.iOS.Services;
using GoogleContacts.Services;

[assembly:Xamarin.Forms.Dependency(typeof(TokenService))]
namespace GoogleContacts.iOS.Services
{
	public class TokenService : ITokenService
	{
		public static NSString kTokenStateKey = (NSString)"tokenState";

		public void SetToken(NSString token)
		{
			if (!string.IsNullOrWhiteSpace(token))
			{
				NSUserDefaults.StandardUserDefaults[kTokenStateKey] = token;
			}
			else 
			{
				NSUserDefaults.StandardUserDefaults.RemoveObject(kTokenStateKey);
			}
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		public string GetToken()
		{
			var token = (NSString)NSUserDefaults.StandardUserDefaults[kTokenStateKey];
			return token.ToString();
		}

		public static NSString kRefreshTokenStateKey = (NSString)"refreshTokenState";

		public void SetRefreshToken(NSString token)
		{
			if (!string.IsNullOrWhiteSpace(token))
			{
				NSUserDefaults.StandardUserDefaults[kRefreshTokenStateKey] = token;
			}
			else
			{
				NSUserDefaults.StandardUserDefaults.RemoveObject(kRefreshTokenStateKey);
			}
			NSUserDefaults.StandardUserDefaults.Synchronize();
		}

		public string GetRefreshToken()
		{
			var token = (NSString)NSUserDefaults.StandardUserDefaults[kRefreshTokenStateKey];
			return token.ToString();
		}
	}
}
