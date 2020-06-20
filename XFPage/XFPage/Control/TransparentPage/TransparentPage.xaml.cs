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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Preserve(AllMembers = true)]
    public partial class TransparentPage : ContentPage
    {
        #region Constructor

        public TransparentPage()
        {
            InitializeComponent();
        }

        #endregion

        #region Events

        public event EventHandler<GestureEventArgs> ItemClicked;

        #endregion

        #region Public Methods

        public void RaiseItemClicked(Point point)
        {
            var eventargs = new GestureEventArgs() { GesturePoint = point };
            ItemClicked?.Invoke(this, eventargs);
        }

        #endregion
    }
}