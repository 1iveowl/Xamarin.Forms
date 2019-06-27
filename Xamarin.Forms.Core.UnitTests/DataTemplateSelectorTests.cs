using System;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class DataTemplateSelectorTests : BaseTestFixture
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		class TemplateOne : DataTemplate
		{
			public TemplateOne () : base (typeof (ViewCell))
			{
				
			}
		}

		class TemplateTwo : DataTemplate
		{
			public TemplateTwo () : base (typeof (EntryCell))
			{
				
			}
		}

		class TemplateNull : DataTemplate
		{
			public TemplateNull () : base (typeof (SwitchCell))
			{
				
			}
		}

		class TestDTS : DataTemplateSelector
		{
			public TestDTS ()
			{
				templateOne = new TemplateOne ();
				templateTwo = new TemplateTwo ();
				templateNull = new TemplateNull ();
			}

			protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
			{
				if (item is null)
					return templateNull;
				if (item is double)
					return templateOne;
				if (item is byte)
					return new TestDTS ();
				return templateTwo;
			}

			readonly DataTemplate templateOne;
			readonly DataTemplate templateTwo;
			readonly DataTemplate templateNull;
		}

		class TestHFDTS : DataTemplateSelector
		{
			protected override DataTemplate OnSelectTemplate (object item, BindableObject container)
			{
				if (item is null)
					return new DataTemplate(typeof(Entry));
				return new DataTemplate(typeof(Label));
			}
		}

		[Test]
		public void Constructor ()
		{
			var dts = new TestDTS ();
		}

		[Test]
		public void ReturnsCorrectType ()
		{
			var dts = new TestDTS ();
			Assert.IsInstanceOf<TemplateOne> (dts.SelectTemplate (1d, null));
			Assert.IsInstanceOf<TemplateTwo> (dts.SelectTemplate ("test", null));
			Assert.IsInstanceOf<TemplateNull> (dts.SelectTemplate (null, null));
		}

		[Test]
		public void ListViewSupport ()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);
			listView.ItemsSource = new object[] { 0d, "test", null };

			listView.ItemTemplate = new TestDTS ();
			Assert.IsInstanceOf<ViewCell> (listView.TemplatedItems[0]);
			Assert.IsInstanceOf<EntryCell> (listView.TemplatedItems[1]);
			Assert.IsInstanceOf<SwitchCell> (listView.TemplatedItems[2]);
		}

		[Test]
		public void ListViewHeaderFooterSupport ()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElement);
			listView.Header = 0d;
			listView.Footer = "footer";

			listView.HeaderTemplate = new TestHFDTS ();
			listView.FooterTemplate = new TestHFDTS ();

			// the list view does not yet pass data to the templates
			// thus the values are always null
			Assert.IsInstanceOf<Entry> (listView.HeaderElement);
			Assert.IsInstanceOf<Entry> (listView.HeaderElement);
		}

		[Test]
		public void NestingThrowsException ()
		{
			var dts = new TestDTS ();
			Assert.Throws<NotSupportedException> (() => dts.SelectTemplate ((byte)0, null));
		}

		[Test]
		public void NullItemAndContainerDoesNotThrow ()
		{
			var dts = new TestDTS ();
			Assert.IsInstanceOf<TemplateNull> (dts.SelectTemplate (null, null));
			Assert.IsInstanceOf<SwitchCell> (dts.CreateContent (null, null));
			Assert.IsInstanceOf<SwitchCell> (dts.CreateContent ());
		}
	}

	[TestFixture]
	public class DataTemplateRecycleTests : BaseTestFixture
	{
		[TearDown]
		public override void TearDown()
		{
			base.TearDown();
			Device.PlatformServices = null;
		}

		[SetUp]
		public override void Setup()
		{
			base.Setup();
			Device.PlatformServices = new MockPlatformServices();
		}

		class TestDataTemplateSelector : DataTemplateSelector
		{
			readonly DataTemplate declarativeTemplate;
			readonly DataTemplate proceduralTemplate;

			public TestDataTemplateSelector ()
			{
				declarativeTemplate = new DataTemplate(typeof(ViewCell));
				proceduralTemplate = new DataTemplate(() => new EntryCell());
			}

			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
			{
				Counter++;

				if (item is string)
					return declarativeTemplate;

				return proceduralTemplate;
			}

			public int Counter = 0;
		}

		[Test]
		public void ListViewSupport ()
		{
			var listView = new ListView(ListViewCachingStrategy.RecycleElementAndDataTemplate);
			listView.ItemsSource = new object[] { "foo", "bar", 0 };

			Assert.That(listView.CachingStrategy == 
				ListViewCachingStrategy.RecycleElementAndDataTemplate);

			var selector = new TestDataTemplateSelector();
			listView.ItemTemplate = selector;
			Assert.That(selector.Counter == 0);

			Assert.IsInstanceOf<ViewCell>(listView.TemplatedItems[0]);
			Assert.That(selector.Counter == 1);

			Assert.IsInstanceOf<ViewCell>(listView.TemplatedItems[1]);
			Assert.That(selector.Counter == 1);

			Assert.Throws<NotSupportedException>(
				() => { var o = listView.TemplatedItems[2]; });
		}
	}
}