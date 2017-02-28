using Xamarin.Forms;
using GoogleContacts.Services;
using GoogleContacts.ViewModels;
using GoogleContacts.ValueConverters;
using GoogleContacts.Models;

namespace GoogleContacts.Pages
{
	public class ContactsPage : ContentPage
	{
		#region Properties

		ContactsPageViewModel _viewModel
		{
			get { return BindingContext as ContactsPageViewModel; }
		}

		#endregion

		#region Constructors

		public ContactsPage()
		{
			Title = "Contacts";

			var exit = new ToolbarItem
			{
				Text = "Logout"
			};
			exit.SetBinding(MenuItem.CommandProperty, "LogOut");
			ToolbarItems.Add(exit);

			BindingContext = new ContactsPageViewModel(DependencyService.Get<INavService>());

			var itemTemplate = new DataTemplate(typeof(ImageCell));
			itemTemplate.SetBinding(TextCell.TextProperty, "DisplayName");
			itemTemplate.SetBinding(ImageCell.ImageSourceProperty, "ImageUrl");

			var contactList = new ListView
			{
				ItemTemplate = itemTemplate,
				SeparatorColor = (Device.OS == TargetPlatform.iOS) ? Color.FromHex("#DCE4EB") : Color.Black
			};

			contactList.SetBinding(ItemsView<Cell>.ItemsSourceProperty, "Contacts");
			contactList.SetBinding(IsVisibleProperty, "IsProcessBusy", converter: new BooleanConverter());

			contactList.ItemTapped += (sender, e) =>
			{
				var item = (Person)e.Item;
				if (item == null) return;
				_viewModel.PersonDetails.Execute(item);
				item = null;
			};

			var activityIndicator = new ActivityIndicator
			{
				IsRunning = true
			};
			var loadingLabel = new Label
			{
				FontSize = 14,
				FontAttributes = FontAttributes.Bold,
				TextColor = Color.Black,
				HorizontalTextAlignment = TextAlignment.Center,
				Text = "Loading Contacts..."
			};
			var progressIndicator = new StackLayout
			{
				Orientation = StackOrientation.Vertical,
				HorizontalOptions = LayoutOptions.CenterAndExpand,
				VerticalOptions = LayoutOptions.CenterAndExpand,
				Children =
				{
					activityIndicator,
					loadingLabel
				}
			};
			progressIndicator.SetBinding(IsVisibleProperty, "IsProcessBusy");

			var mainLayout = new StackLayout
			{
				Children =
				{
					contactList,
					progressIndicator
				}
			};

			Content = mainLayout;
		}

		#endregion

		#region Events

		protected override async void OnAppearing()
		{
			base.OnAppearing();

			if (_viewModel != null)
				await _viewModel.Init();
		}

		#endregion
	}
}

