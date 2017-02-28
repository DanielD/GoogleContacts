using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GoogleContacts.Models;
using GoogleContacts.Services;
using Xamarin.Forms;

namespace GoogleContacts.ViewModels
{
	public class ContactsPageViewModel : BaseViewModel
	{
		#region Members

		ObservableCollection<Person> _contacts;

		#endregion

		#region Properties

		/// <summary>
		/// Contacts
		/// </summary>
		public ObservableCollection<Person> Contacts
		{
			get { return _contacts; }
			set
			{
				if (_contacts == value) return;
				_contacts = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		public ContactsPageViewModel(INavService navService) : base(navService)
		{
			Contacts = new ObservableCollection<Person>();
		}

		#endregion

		#region Methods

		public override async Task Init()
		{
			await LoadContacts();
		}

		async Task LoadContacts()
		{
			if (IsProcessBusy) return;

			IsProcessBusy = true;

			try
			{
				var token = TokenService.GetToken();
				Contacts = new ObservableCollection<Person>(await GoogleDataService.GetContacts(token));
			}
			finally
			{
				IsProcessBusy = false;
			}
		}

		#endregion

		#region Commands

		Command<Person> _personDetails;
		public Command<Person> PersonDetails
		{
			get 
			{
				return _personDetails ?? (_personDetails = new Command<Person>(async (personDetails) => await NavService.NavigateToViewModel<ContactPageViewModel, Person>(personDetails)));
			}
		}

		#endregion
	}
}
