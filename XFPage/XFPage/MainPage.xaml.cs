using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
using XFPage.Control;
using XFPage.Views;

namespace XFPage
{
    [DesignTimeVisible(false)]
    [Preserve(AllMembers = true)]
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : TabPage
    {
        #region Constructor

        public MainPage()
        {
            InitializeComponent();
            TabbedPageTransforms.TabIconClicked += TabbedPageTransforms_TabIconClicked;
        }

        #endregion

        #region CallBack Methods

        private async void TabbedPageTransforms_TabIconClicked(object sender, Helper.TabsEventArgs e)
        {
            var selectedPage = TabbedPageTransforms.GetChildPageWithTransform(this, e.SelectedIndex);
            var lastSelectedPage = TabbedPageTransforms.GetChildPageWithTransform(this, e.LastSelectedIndex);
            if (selectedPage != null)
            {
                if (selectedPage.Title == "Popup")
                    await Navigation.PushModalAsync(new TransitionNavigationPage(true, new HalfPopUpPage() { Title = selectedPage.Title, BindingContext = this.BindingContext }), true);
                else if (selectedPage.Title == "PopupMenu")
                    await Navigation.PushModalAsync(new TransitionNavigationPage(true, new FullPopUpPage(this.BottomBarHeight) { Title = selectedPage.Title, BindingContext = this.BindingContext }), true);
            }
        }

        #endregion
    }
}