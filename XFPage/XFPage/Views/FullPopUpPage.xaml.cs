using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XFPage.Views 
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class FullPopUpPage : ContentPage
    {
        #region Constructor

        public FullPopUpPage()
        {
            InitializeComponent();
        }

        public FullPopUpPage(double bottombarheight) 
        {
            InitializeComponent();
            //bottomBarrow.Height = bottombarheight;
        }

        #endregion
    }
}