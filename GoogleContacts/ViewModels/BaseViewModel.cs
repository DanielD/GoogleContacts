using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GoogleContacts.Services;
using Xamarin.Forms;

namespace GoogleContacts.ViewModels
{
	public abstract class BaseViewModel : INotifyPropertyChanged
	{
		#region Members

		bool _isProcessBusy;
		IDataService _googleDataService;
		ITokenService _tokenService;

		#endregion
		
		#region Properties

		protected INavService NavService { get; private set; }

		/// <summary>
		/// IsProcessBusy
		/// </summary>
		public bool IsProcessBusy
		{
			get { return _isProcessBusy; }
			set
			{
				if (value.Equals(_isProcessBusy)) return;
				_isProcessBusy = value;
				OnPropertyChanged();
				OnIsBusyChanged();
			}
		}

		/// <summary>
		/// GoogleDataService
		/// </summary>
		public IDataService GoogleDataService
		{
			get { return _googleDataService; }
			set
			{
				if (_googleDataService == value) return;
				_googleDataService = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// TokenService
		/// </summary>
		protected ITokenService TokenService
		{
			get { return _tokenService; }
			set
			{
				if (_tokenService == value) return;
				_tokenService = value;
				OnPropertyChanged();
			}
		}

		#endregion

		#region Constructors

		protected BaseViewModel(INavService navService)
		{
			NavService = navService;

			TokenService = DependencyService.Get<ITokenService>();
			GoogleDataService = new GoogleDataService();
		}

		#endregion

		#region Events

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		protected virtual void OnIsBusyChanged() {}

		#endregion

		#region Methods

		public abstract Task Init();

		public void DisplayAlert(string title, string message)
		{
			string[] values = { title, message };
			MessagingCenter.Send<BaseViewModel, string[]>(this, "DisplayAlert", values);
		}

		#endregion

		#region Commands

		Command _logOut;
		public Command LogOut
		{
			get 
			{
				return _logOut ?? (_logOut = new Command(async () => await NavService.LogOut()));
			}
		}

		#endregion

		#region INotifyPropertyChanged Implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}

	public abstract class BaseViewModel<ViewParam> : BaseViewModel
	{
		#region Constructors

		protected BaseViewModel(INavService navService) : base(navService) {}

		#endregion

		#region Methods

		public override async Task Init()
		{
			await Init(default(ViewParam));
		}

		public abstract Task Init(ViewParam item);

		#endregion
	}
}
