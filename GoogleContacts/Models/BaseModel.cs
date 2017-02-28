using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GoogleContacts
{
	public abstract class BaseModel : INotifyPropertyChanged
	{
		#region Events

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		#endregion

		#region INotifyPropertyChanged Implementation

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
	}
}
