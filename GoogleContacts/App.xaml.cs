using Xamarin.Forms;
using GoogleContacts.Pages;
using GoogleContacts.Services;
using GoogleContacts.ViewModels;

namespace GoogleContacts
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();
		}

		protected override void OnStart()
		{
			// Handle when your app starts
		}

		protected override void OnSleep()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume()
		{
			// Handle when your app resumes
		}

		public static Page GetContactsPage()
		{
			var formsPage = new NavigationPage(new ContactsPage
			{
				Title = "Contacts"
			})
			{
				BarBackgroundColor = Color.FromHex("#8491F9"),
				BarTextColor = Color.White
			};

			var navService = DependencyService.Get<INavService>() as NavService;
			navService.Navigation = formsPage.Navigation;

			navService.RegisterViewMapping(typeof(ContactsPageViewModel), typeof(ContactsPage));
			navService.RegisterViewMapping(typeof(ContactPageViewModel), typeof(ContactPage));

			return formsPage;
		}
	}
}
