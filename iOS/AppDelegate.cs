using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;

using OpenId.AppAuth;
using GoogleContacts.iOS.Views;
using Xamarin.Forms;

namespace GoogleContacts.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		public IAuthorizationFlowSession CurrentAuthorizationFlow { get; set; }

		UIWindow window;

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();
			ImageCircle.Forms.Plugin.iOS.ImageCircleRenderer.Init();

			//LoadApplication(new App());

			//return base.FinishedLaunching(app, options);

			window = new UIWindow(UIScreen.MainScreen.Bounds);
			var nav = new UINavigationController(new GoogleViewController());
			nav.NavigationBar.BarTintColor = UIColor.Clear.FromHex("#8491F9");
			window.RootViewController = nav;
			window.MakeKeyAndVisible();

			return true;
		}

		public override bool OpenUrl(UIApplication application, NSUrl url, string sourceApplication, NSObject annotation)
		{
			if (CurrentAuthorizationFlow?.ResumeAuthorizationFlow(url) == true)
			{
				return true;
			}

			//

			return false;
		}
	}
}
