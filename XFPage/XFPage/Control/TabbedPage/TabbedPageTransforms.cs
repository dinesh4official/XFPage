using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XFPage.Helper;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

namespace XFPage.Control 
{
    [Preserve(AllMembers = true)]
    public static class TabbedPageTransforms 
    {
        #region Bindable Properties

        public static BindableProperty HideTitleProperty = BindableProperty.CreateAttached("HideTitle", typeof(bool), typeof(TabbedPageTransforms), default(bool), BindingMode.OneWay);

        public static BindableProperty DisableLoadProperty = BindableProperty.CreateAttached("DisableLoad", typeof(bool), typeof(TabbedPageTransforms), default(bool), BindingMode.OneWay);

        public static BindableProperty DisableSelectionProperty = BindableProperty.CreateAttached("DisableSelection", typeof(bool), typeof(TabbedPageTransforms), false, BindingMode.OneWay);

        public static BindableProperty TabClickedCommandProperty = BindableProperty.CreateAttached("TabClickedCommand", typeof(Command<TabsEventArgs>), typeof(TabbedPageTransforms), null, BindingMode.OneWay);

        #endregion

        #region Events & Command

        public static event EventHandler<TabsEventArgs> TabIconClicked;

        #endregion

        #region Static Methods

        public static Command<TabsEventArgs> TabClickedCommand { get; set; }

        public static Command<TabsEventArgs> GetTabClickedCommand(BindableObject view)
        {
            return (Command<TabsEventArgs>)view.GetValue(TabClickedCommandProperty);
        }

        public static void SetTabClickedCommand(BindableObject view, Command<TabsEventArgs> value)
        {
            view.SetValue(TabClickedCommandProperty, value);
        }

        public static bool GetHideTitle(BindableObject view)
        {
            return (bool)view.GetValue(HideTitleProperty);
        }

        public static void SetHideTitle(BindableObject view, bool value)
        {
            view.SetValue(HideTitleProperty, value);
        }

        public static bool GetDisableLoad(BindableObject view)
        {
            return (bool)view.GetValue(DisableLoadProperty);
        }

        public static void SetDisableLoad(BindableObject view, bool value)
        {
            view.SetValue(DisableLoadProperty, value);
        }

        public static bool GetDisableSelection(BindableObject view)
        {
            return (bool)view.GetValue(DisableSelectionProperty);
        }

        public static void SetDisableSelection(BindableObject view, bool value)
        {
            view.SetValue(DisableSelectionProperty, value);
        }

        public static Page GetChildPageWithTransform(this Xamarin.Forms.TabbedPage parentTabbedPage, int tabIndex)
        {
            if (tabIndex < 0)
                return null;

            var element = parentTabbedPage.Children[tabIndex];
            return GetPageWithTransform(element);
        }

        public static Page GetPageWithTransform(this Page element)
        {
            if (GetDisableLoad(element) != (bool)DisableLoadProperty.DefaultValue)
                return element;

            if (element is NavigationPage navigationPage)
                return navigationPage.RootPage;

            return element;
        }

        public static bool TabIconClickedFunction(int newIndex, int oldIndex, object sender)
        {
            var eventargs = new TabsEventArgs() { SelectedIndex = newIndex, LastSelectedIndex = oldIndex };
            TabIconClicked?.Invoke(sender, eventargs);
            TabClickedCommand?.Execute(eventargs);
            return eventargs.Cancel;
        }

        #endregion
    }
}