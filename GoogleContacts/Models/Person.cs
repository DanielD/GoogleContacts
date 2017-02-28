using System.Collections.Generic;

namespace GoogleContacts.Models
{
	public class Person : BaseModel
	{
		#region Members

		private string _id;
		private string _firstName;
		private string _lastName;
		private string _imageUrl;
		private PersonDetails _details;

		#endregion
		
		#region Properties

		/// <summary>
		/// Id
		/// </summary>
		public string Id
		{
			get { return _id; }
			set
			{
				if (value.Equals(_id)) return;
				_id = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// FirstName
		/// </summary>
		public string FirstName
		{
			get { return _firstName; }
			set
			{
				if (value.Equals(_firstName)) return;
				_firstName = value;
				OnPropertyChanged();
				OnPropertyChanged("DisplayName");
			}
		}

		/// <summary>
		/// LastName
		/// </summary>
		public string LastName
		{
			get { return _lastName; }
			set
			{
				if (value.Equals(_lastName)) return;
				_lastName = value;
				OnPropertyChanged();
				OnPropertyChanged("DisplayName");
			}
		}

		/// <summary>
		/// DisplayName
		/// </summary>
		public string DisplayName
		{
			get { return FirstName + " " + LastName; }
		}

		/// <summary>
		/// ImageUrl
		/// </summary>
		public string ImageUrl
		{
			get { return _imageUrl; }
			set
			{
				if (value.Equals(_imageUrl)) return;
				_imageUrl = value;
				OnPropertyChanged("ImageUrl");
			}
		}

		/// <summary>
		/// Details
		/// </summary>
		public PersonDetails Details
		{
			get { return _details; }
			set
			{
				if (value.Equals(_details)) return;
				_details = value;
				OnPropertyChanged("Details");
			}
		}

		#endregion
	}

	public class PersonDetails : BaseModel
	{
		#region Properties

		public IList<PhoneNumber> PhoneNumbers { get; set; }
		public IList<EmailAddress> EmailAddresses { get; set; }
		
		private Organization _organization;
		/// <summary>
		/// Organization
		/// </summary>
		public Organization Organization
		{
			get { return _organization; }
			set
			{
				if (value.Equals(_organization)) return;
				_organization = value;
				OnPropertyChanged("Organization");
			}
		}

		#endregion
	}

	public class Organization : BaseModel
	{
		#region Properties
		
		private string _name;
		/// <summary>
		/// Name
		/// </summary>
		public string Name
		{
			get { return Name; }
			set
			{
				if (value.Equals(_name)) return;
				_name = value;
				OnPropertyChanged("Name");
				OnPropertyChanged("DisplayTitle");
			}
		}
		
		private string _title;
		/// <summary>
		/// Title
		/// </summary>
		public string Title
		{
			get { return _title; }
			set
			{
				if (value.Equals(_title)) return;
				_title = value;
				OnPropertyChanged("Title");
				OnPropertyChanged("DisplayTitle");
			}
		}

		public string DisplayTitle
		{
			get { return (string.IsNullOrWhiteSpace(Title)) ? Name : Title; }
		}

		#endregion
	}

	public class PhoneNumber
	{
		#region Properties

		public PhoneType Type { get; set; }
		public string Value { get; set; }
		public string CanonicalForm { get; set; }

		#endregion
	}

	public class EmailAddress
	{
		#region Properties

		public EmailType Type { get; set; }
		public string Value { get; set; }

		#endregion
	}

	public enum PhoneType
	{
		Home = 1,
		Work = 2,
		Mobile = 3,

		Other = 99
	}

	public enum EmailType
	{
		Home = 1,
		Work = 2,

		Other = 99
	}
}
