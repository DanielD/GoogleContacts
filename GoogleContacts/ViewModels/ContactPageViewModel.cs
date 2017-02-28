using System.Threading.Tasks;
using GoogleContacts.Models;
using GoogleContacts.Services;
using Xamarin.Forms;
using Plugin.Messaging;

namespace GoogleContacts.ViewModels
{
	public class ContactPageViewModel : BaseViewModel<Person>
	{
		#region Members

		Person _person;

		#endregion

		#region Properties

		public Person Person
		{
			get { return _person; }
			set 
			{
				if (_person == value) return;
				_person = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		public ContactPageViewModel(INavService navService) : base(navService) {}

		#endregion

		#region Methods

		public override async Task Init(Person item)
		{
			var token = TokenService.GetToken();
			await GoogleDataService.LoadContactDetails(token, item);
			Person = item;

			// temporary until Google People API is fixed
			Person = new Person
			{
				FirstName = "Daniel",
				LastName = "Dhillon",
				ImageUrl = "https://lh5.googleusercontent.com/-kv6ZbOeIPA0/AAAAAAAAAAI/AAAAAAAAAI0/1aoWlBq6pes/photo.jpg",
				Details = new PersonDetails
				{
					EmailAddresses = new[] {
						new EmailAddress
						{
							Type = EmailType.Home,
							Value = "daniel@email.xyz"
						},
						new EmailAddress
						{
							Type = EmailType.Other,
							Value = "daniel@email.abc"
						}
					},
					PhoneNumbers = new[] {
						new PhoneNumber
						{
							Type = PhoneType.Mobile,
							CanonicalForm = "19095551212",
							Value = "(909) 555-1212"
						},
						new PhoneNumber
						{
							Type = PhoneType.Work,
							CanonicalForm = "19095551313",
							Value = "(909) 555-1313"
						}
					},
					Organization = new Organization
					{
						Name = "Dhillon Zone",
						Title = "Software Developer"
					}
				}
			};
		}

		#endregion

		#region Commands

		
		private Command _phoneNumberCommand;
		/// <summary>
		/// PhoneNumberCommand
		/// </summary>
		public Command PhoneNumberCommand
		{
			get 
			{
				return _phoneNumberCommand ?? (_phoneNumberCommand = new Command((phoneNumber) =>
				{
					var phoneCall = MessagingPlugin.PhoneDialer;
					if (phoneCall.CanMakePhoneCall)
						phoneCall.MakePhoneCall((string)phoneNumber);
					else
						DisplayAlert("Alert", "Unable to call number");
				}));
			}
		}
		
		private Command _smsCommand;
		/// <summary>
		/// SmsCommand
		/// </summary>
		public Command SmsCommand
		{
			get
			{
				return _smsCommand ?? (_smsCommand = new Command((phoneNumber) =>
				{
					var smsMessenger = MessagingPlugin.SmsMessenger;
					if (smsMessenger.CanSendSms)
						smsMessenger.SendSms((string)phoneNumber, "Hello from Xamarin.Forms!");
					else
						DisplayAlert("Alert", "Unable to send SMS");
				}));
			}
		}
		
		private Command _sendEmailCommand;
		/// <summary>
		/// SendEmailCommand
		/// </summary>
		public Command SendEmailCommand
		{
			get
			{
				return _sendEmailCommand ?? (_sendEmailCommand = new Command((emailAddress) =>
				{
					var email = MessagingPlugin.EmailMessenger;
					if (email.CanSendEmail)
						email.SendEmail((string)emailAddress, "Test from Google Contacts", "Sent via Xam.MessagingPlugin.");
					else
						DisplayAlert("Alert", "Unable to send email");
				}));
			}
		}

		#endregion
	}
}
