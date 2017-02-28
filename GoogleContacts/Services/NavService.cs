using System;
using Xamarin.Forms;
using GoogleContacts.Services;
using System.Collections.Generic;
using GoogleContacts.ViewModels;
using System.Threading.Tasks;
using System.Reflection;
using System.Linq;
using GoogleContacts.Pages;

[assembly: Dependency(typeof(NavService))]
namespace GoogleContacts.Services
{
	public class NavService : INavService
	{
		#region Members

		readonly IDictionary<Type, Type> _viewMapping = new Dictionary<Type, Type>();

		#endregion

		#region Properties

		public INavigation Navigation { get; set; }

		#endregion

		#region Methods

		public void RegisterViewMapping(Type viewModel, Type view)
		{
			_viewMapping.Add(viewModel, view);
		}

		#endregion

		#region INavService Implementation

		public async Task BackToMainPage()
		{
			await Navigation.PopToRootAsync(true);
		}

		public async Task NavigateToViewModel<ViewModel, ViewParam>(ViewParam parameter) where ViewModel : BaseViewModel
		{
			Type viewType;

			if (_viewMapping.TryGetValue(typeof(ViewModel), out viewType))
			{
				var constructor = viewType.GetTypeInfo()
										  .DeclaredConstructors
										  .FirstOrDefault(dc => dc.GetParameters().Count() <= 0);

				var view = constructor.Invoke(null) as Page;
				await Navigation.PushAsync(view, true);
			}

			if (Navigation.NavigationStack.Last().BindingContext is BaseViewModel<ViewParam>)
				await ((BaseViewModel<ViewParam>)(Navigation.NavigationStack.Last().BindingContext)).Init(parameter);
		}

		public async Task PreviousPage()
		{
			if (Navigation.NavigationStack != null && Navigation.NavigationStack.Count > 0)
			{
				await Navigation.PopAsync(true); 
			}
		}

		public void ClearAllViewFromStack()
		{
			if (Navigation.NavigationStack.Count <= 1)
				return;

			for (var i = 0; i < Navigation.NavigationStack.Count - 1; i++)
				Navigation.RemovePage(Navigation.NavigationStack[i]);
		}

		public async Task LogOut()
		{
			ClearAllViewFromStack();

			await Navigation.PushAsync(new GoogleLoginPage());
		}

		#endregion
	}
}
