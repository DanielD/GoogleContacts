using System;

namespace GoogleContacts.Services
{
	public interface ITokenService
	{
		string GetToken();
		string GetRefreshToken();
	}
}
