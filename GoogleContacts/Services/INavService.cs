using System.Threading.Tasks;
using GoogleContacts.ViewModels;

namespace GoogleContacts.Services
{
	public interface INavService
	{
		// Navigate back to the Previous page in the NavigationStack
		Task PreviousPage();

		// Navigate to the first page within the NavigationStack 
		Task BackToMainPage();

		Task NavigateToViewModel<ViewModel, ViewParam>(ViewParam parameter) where ViewModel : BaseViewModel;

		Task LogOut();

		void ClearAllViewFromStack();
	}
}
