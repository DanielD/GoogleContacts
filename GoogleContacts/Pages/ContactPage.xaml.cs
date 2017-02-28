using GoogleContacts.Services;
using GoogleContacts.ViewModels;
using Xamarin.Forms;

namespace GoogleContacts
{
	public partial class ContactPage : ContentPage
	{
		#region Members

		ContactPageViewModel _viewModel
		{
			get { return BindingContext as ContactPageViewModel; }
		}

		#endregion

		#region Constructors
		
		public ContactPage()
		{
			InitializeComponent();

			BindingContext = new ContactPageViewModel(DependencyService.Get<INavService>());

			MessagingCenter.Subscribe<BaseViewModel, string[]>(this, "DisplayAlert", (sender, values) =>
			{
				DisplayAlert(values[0], values[1], "Ok");
			});
		}

		#endregion
	}
}
