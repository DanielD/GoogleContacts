using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GoogleContacts.Models;
using System.Diagnostics;

namespace GoogleContacts.Services
{
	public class GoogleDataService : WebService, IDataService
	{
		#region Members

		const string kKey = "<Key>";

		#endregion

		#region IDataService Implementation

		public async Task<List<Person>> GetContacts(string token)
		{
			try
			{
				var url = new Uri($"https://content-people.googleapis.com/v1/people/me/connections?key={kKey}");
				var response = await SendRequestAsync<Google.Apis.People.v1.Data.ListConnectionsResponse>(url, HttpMethod.Get, new Dictionary<string, string> {
					{ "Authorization", $"Bearer {token}" }
				});

				var list = new List<Person>();
				foreach (var connection in response.Connections)
				{
					if (connection.Names != null && connection.Names.Count > 0 && connection.Metadata.Sources.Count(p => p.Type == "PROFILE") > 0)
					{
						list.Add(new Person
						{
							FirstName = connection.Names[0].GivenName,
							LastName = connection.Names[0].FamilyName,
							ImageUrl = connection.Photos[0].Url,
							Id = connection.Metadata.Sources.First(p => p.Type == "PROFILE").Id
						});
					}
				}

				return list;
			}
			catch (Exception ex)
			{
				// log exception
				Debug.WriteLine(ex);

				return null;
			}
		}

		public async Task LoadContactDetails(string token, Person person)
		{
			try
			{
				var url = new Uri($"https://content-people.googleapis.com/v1/people/{person.Id}?key={kKey}");
				var response = await SendRequestAsync<Google.Apis.People.v1.Data.Person>(url, HttpMethod.Get, new Dictionary<string, string> {
					{ "Authorization", $"Bearer {token}" }
				});

				var phoneNumbers = new List<PhoneNumber>();
				foreach (var phoneNumber in response.PhoneNumbers)
				{
					PhoneType phoneType;
					if (!Enum.TryParse(phoneNumber.FormattedType, true, out phoneType))
						phoneType = PhoneType.Other;

					phoneNumbers.Add(new PhoneNumber
					{
						CanonicalForm = phoneNumber.CanonicalForm,
						Value = phoneNumber.Value,
						Type = phoneType
					});
				}

				var emailAddresses = new List<EmailAddress>();
				foreach (var emailAddress in response.EmailAddresses)
				{
					EmailType emailType;
					if (!Enum.TryParse(emailAddress.FormattedType, true, out emailType))
						emailType = EmailType.Other;

					emailAddresses.Add(new EmailAddress
					{
						Value = emailAddress.Value,
						Type = emailType
					});
				}

				var organization = new Organization();
				var o = response.Organizations.FirstOrDefault(p => p.Metadata.Primary == true);
				if (o != null)
				{
					organization.Name = o.Name;
					organization.Title = o.Title;
				}

				person.Details = new PersonDetails
				{
					EmailAddresses = emailAddresses,
					PhoneNumbers = phoneNumbers,
					Organization = organization
				};
			}
			catch (Exception)
			{
				// log exception
			}
		}

		#endregion
	}
}
