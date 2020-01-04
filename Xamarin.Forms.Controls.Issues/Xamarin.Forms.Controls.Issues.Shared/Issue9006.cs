using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 9006, "[Bug] Unable to open a new Page for the second time in Xamarin.Forms Shell Tabbar",
		PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.Shell)]
#endif
	public class Issue9006 : TestShell
	{
		protected override void Init()
		{
			Routing.RegisterRoute("Issue9006_ContentPage", typeof(ContentPage));
			Routing.RegisterRoute("Issue9006_FinalPage", typeof(ContentPage));

			var contentPage = AddBottomTab("Click Me");
			AddBottomTab("Ignore Me");

			Label label = new Label()
			{
				Text = "Clicking on the tab labeled 'Click Me' should pop you back to the root"
			};

			Button button = null;
			button = new Button()
			{
				Text = "Click Me",
				Command = new Command(async () =>
				{
					await GoToAsync("Issue9006_ContentPage");
					await GoToAsync("Issue9006_FinalPage");
					
					button.Text = "Click me again. If pages get pushed again then test has passed.";
					DisplayedPage.Content = new StackLayout()
					{
						Children =
						{
							label
						}
					};
				})
			};

			contentPage.Content = new StackLayout()
			{
				Children =
				{
					button
				}
			};
		}


#if UITEST && __IOS__
		[Test]
		public void ClickingOnTabToPopToRootDoesntBreakNavigation()
		{
		}
#endif
	}
}
