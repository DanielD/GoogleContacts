using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleContacts.Models;

namespace GoogleContacts.Services
{
	public interface IDataService
	{
		Task<List<Person>> GetContacts(string token);
		Task LoadContactDetails(string token, Person person);
	}
}
