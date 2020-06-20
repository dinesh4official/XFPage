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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TransitionNavigationPage : NavigationPage
    {
        #region Bindable Properties

        public static readonly BindableProperty TransitionTypeProperty =
             BindableProperty.Create("TransitionType", typeof(TransitionType), typeof(TransitionNavigationPage), TransitionType.SlideFromRight);

        public TransitionType TransitionType
        {
            get { return (TransitionType)GetValue(TransitionTypeProperty); }
            set { SetValue(TransitionTypeProperty, value); }
        }

        public static readonly BindableProperty NeedTransparentPageProperty =
            BindableProperty.Create("NeedTransparentPage", typeof(bool), typeof(TransitionNavigationPage), false);

        public bool NeedTransparentPage
        {
            get { return (bool)GetValue(NeedTransparentPageProperty); }
            set { SetValue(NeedTransparentPageProperty, value); }
        }

        #endregion

        #region Constructor

        public TransitionNavigationPage() : base()
        {

        }

        public TransitionNavigationPage(Page root) : base(root)
        {

        }

        public TransitionNavigationPage(bool needtransparentPage, Page root) : base(root)
        {
            this.NeedTransparentPage = needtransparentPage;
        }

        #endregion
    }
}