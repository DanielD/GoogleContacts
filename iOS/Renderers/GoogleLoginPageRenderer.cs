using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using GoogleContacts.Pages;
using GoogleContacts.iOS.Renderers;
using UIKit;
using GoogleContacts.iOS.Views;

[assembly: ExportRenderer(typeof(GoogleLoginPage), typeof(GoogleLoginPageRenderer))]
namespace GoogleContacts.iOS.Renderers
{
	public class GoogleLoginPageRenderer : PageRenderer
	{
		UIWindow window;

		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			window = new UIWindow(UIScreen.MainScreen.Bounds);
			window.RootViewController = new UINavigationController(new GoogleViewController());
			window.MakeKeyAndVisible();
		}
	}
}
