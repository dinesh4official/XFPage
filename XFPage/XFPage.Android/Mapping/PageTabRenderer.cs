using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using XFPage.Control;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Android.Support.Design.BottomNavigation;
using Android.Support.Design.Internal;
using Android.Support.Design.Widget;
using Android.Widget;
using XFPage.Droid.Mapping;
using Xamarin.Forms.Platform.Android.AppCompat;
using Xamarin.Forms.PlatformConfiguration.AndroidSpecific;
using Xamarin.Forms.Platform.Android;
using UIView = Android.Views.View;
using Android.Graphics.Drawables;
using Android.Content.Res;
using XFPage;
using XFPage.Helper;
using XFPage.Droid.Control;
using System.Threading.Tasks;
using XFPage.Droid.Helper; 

[assembly: ExportRenderer(typeof(TabPage), typeof(PageTabRenderer))]
namespace XFPage.Droid.Mapping
{
    [Preserve(AllMembers = true)]
    public class PageTabRenderer : TabbedPageRenderer, BottomNavigationView.IOnNavigationItemSelectedListener
    {
        #region Fields

        private ViewGroup _bottomTabStrip;
        private BottomNavigationView _bottomNavigationView;
        private const int DeleayBeforeTabAdded = 10;
        protected readonly Dictionary<Element, BadgeView> BadgeViews = new Dictionary<Element, BadgeView>();
        private int tabCount;
        private int selectedTabId = -1;
        private int lastactiveTabId = -1;
        private bool isdisabledTabpressed = false;

        #endregion

        #region Properties

        public TabPage TabPage { get { return this.Element as TabPage; } }

        #endregion

        #region Constructor

        public PageTabRenderer(Context context) : base(context)
        {
        
        }

        #endregion

        #region Interface

        public new bool OnNavigationItemSelected(IMenuItem item)
        {
            if (Element == null)
                return false;

            var id = item.ItemId;
            var tabnavigationresult = TabbedPageTransforms.TabIconClickedFunction(id, selectedTabId, this);
            if (tabnavigationresult)
                return false;

            var page = Element.GetChildPageWithTransform(id);
            var ispagedisabled = TabbedPageTransforms.GetDisableLoad(page);
            this.UpdateTabSelection(id, page, ispagedisabled);
            if (!ispagedisabled)
            {
                if (_bottomNavigationView != null)
                {
                    if (_bottomNavigationView.SelectedItemId != id && Element.Children.Count > id && id >= 0)
                    {
                        Element.CurrentPage = Element.Children[id];
                        UpdateIconsAsync(id);
                    }
                    else if (_bottomNavigationView.SelectedItemId == id)
                        UpdateIconsAsync(id);
                    else
                        UpdateIconsAsync(-1);
                }

                return true;
            }

            return false;
        }

        #endregion

        #region Override Methods

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.TabbedPage> e)
        {
            base.OnElementChanged(e);

            Cleanup(e.OldElement);
            Cleanup(Element);
            tabCount = InitLayout();

            for (var i = 0; i < tabCount; i++)
                AddTabBadge(i);

            Element.ChildAdded += OnTabAdded;
            Element.ChildRemoved += OnTabRemoved;
            this.UpdateCurrentPage();
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == "CurrentPage")
            {
                this.UpdateTabs();
            }
            else if (e.PropertyName == TabPage.IsSelectionChangedProperty.PropertyName && TabPage.IsSelectionChanged)
            {
                var currentPage = this.Element.CurrentPage;
                var itemId = this.Element.Children.IndexOf(currentPage);
                this.UpdateTabSelection(itemId, currentPage, TabbedPageTransforms.GetDisableLoad(currentPage));
            }
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (!changed)
                return;

            var density = Resources.DisplayMetrics.Density;
            if (_bottomTabStrip != null && TabPage != null)
            {
                var bottomHeight = _bottomTabStrip.Height / density;
                TabPage.BottomBarHeight = bottomHeight;

                for (int i = 0; i < _bottomTabStrip.ChildCount; i++)
                {
                    Rect rect = new Rect();
                    var formsrect = new Rectangle();
                    var menuItem = _bottomTabStrip.GetChildAt(i) as BottomNavigationItemView;
                    var title = menuItem.ItemData.Title;
                    menuItem.GetGlobalVisibleRect(rect);
                    formsrect.X = rect.Left / density;
                    formsrect.Y = rect.Top / density;
                    formsrect.Width = (rect.Right - rect.Left) / density;
                    formsrect.Height = formsrect.Y + bottomHeight;

                    if (TabPage.TabItemBounds.ContainsKey(title))
                        TabPage.TabItemBounds.Remove(title);

                    TabPage.TabItemBounds.Add(title, formsrect);
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            Cleanup(Element);
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private int InitLayout()
        {
            switch (this.Element.OnThisPlatform().GetToolbarPlacement())
            {
                case ToolbarPlacement.Default:
                case ToolbarPlacement.Bottom:
                    _bottomTabStrip = ViewGroup.FindChildOfType<BottomNavigationView>()?.GetChildAt(0) as ViewGroup;
                    _bottomNavigationView = ViewGroup.FindChildOfType<BottomNavigationView>();
                    if (_bottomNavigationView != null)
                    {
                        _bottomNavigationView.SetOnNavigationItemSelectedListener(this);
                        _bottomNavigationView.ItemIconTintList = null;
                        _bottomNavigationView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
                        _bottomNavigationView.SetShiftMode(false, false);
                    }
                    if (_bottomTabStrip == null)
                    {
                        Console.WriteLine("No bottom tab layout found. Badge not added.");
                        return 0;
                    }
                    return _bottomTabStrip.ChildCount;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateCurrentPage()
        {
            tabCount = InitLayout();
            var page = Element.CurrentPage;
            var selectedIndex = Element.Children.IndexOf(page);
            UpdateIconsAsync(selectedIndex);
            this.UpdateTabSelection(selectedIndex, page, TabbedPageTransforms.GetDisableLoad(page));
        }

        private void UpdateIconsAsync(int selectedIndex)
        {
            var menu = _bottomNavigationView.Menu;
            for (int i = 0; i < Element.Children.Count; i++)
            {
                var page = Element.GetChildPageWithTransform(i);
                this.UpdateMenuIcon(i, menu, page.IconImageSource);
            }

            UpdateTabs();
        }

        private async Task<Drawable> IdFromTitleAsync(ImageSource imageSource)
        {
            if (imageSource is FontImageSource)
                return await DeviceHelper.MainActivity.BaseContext.GetFormsDrawableAsync(imageSource);

            return null;
        }

        private async void UpdateMenuIcon(int index, IMenu menu, ImageSource imageSource, bool needtoselect = false)
        {
            if (imageSource == null)
                menu.GetItem(index).SetIcon(null);
            else if (imageSource is FontImageSource)
            {
                var drawable = await IdFromTitleAsync(imageSource);
                menu.GetItem(index).SetIcon(drawable);
                drawable?.Dispose();
            }
            else
                menu.GetItem(index).SetIcon(ImageSourceIdFromTitleAsync(imageSource, ResourceManager.DrawableClass));
        }

        private int ImageSourceIdFromTitleAsync(ImageSource imageSource, Type type)
        {
            if (imageSource is FileImageSource)
            {
                string name = System.IO.Path.GetFileNameWithoutExtension((FileImageSource)imageSource);
                int id = GetId(type, name);
                return id;
            }
            else if (imageSource is StreamImageSource)
            {
                throw new Exception("StreamImageSource not yet supported");
            }
            else if (imageSource is UriImageSource)
            {
                throw new Exception("UriImageSource not yet supported");
            }

            return 0;
        }

        private void UpdateTabs()
        {
            for (var i = 0; i < tabCount; i++)
                UpdateTab(i);
        }

        int GetId(Type type, string memberName)
        {
            object value = type.GetFields().FirstOrDefault(p => p.Name == memberName)?.GetValue(type)
                ?? type.GetProperties().FirstOrDefault(p => p.Name == memberName)?.GetValue(type);
            if (value is int)
                return (int)value;
            return 0;
        }

        private void UpdateTab(int tabIndex)
        {
            var page = Element.GetChildPageWithTransform(tabIndex);
            var menu = (BottomNavigationMenuView)_bottomNavigationView.GetChildAt(0);
            var isTitledisabled = TabbedPageTransforms.GetHideTitle(page);
            var view = menu.GetChildAt(tabIndex);
            if (view == null) return;
            if (view is BottomNavigationItemView itemView)
            {
                itemView.SetShifting(false);
                itemView.SetBackground(null);
                itemView.SetBackgroundColor(Android.Graphics.Color.Transparent);
                for (int j = 0; j < itemView.ChildCount; j++)
                {
                    UIView childView = itemView.GetChildAt(j);
                    if (childView is BaselineLayout baselineLayout)
                    {
                        if (isTitledisabled)
                        {
                            childView.Visibility = ViewStates.Gone;
                            childView.SetPadding(0, 0, 0, 0);
                        }

                        for (int z = 0; z < baselineLayout.ChildCount; z++)
                        {
                            var textview = baselineLayout.GetChildAt(z);
                            textview.SetPadding(0, 0, 0, 0);
                            if (isTitledisabled)
                                textview.Visibility = ViewStates.Gone;
                        }
                    }
                    else if (isTitledisabled && childView is ImageView icon)
                    {
                        FrameLayout.LayoutParams parames = (FrameLayout.LayoutParams)icon.LayoutParameters;
                        parames.Height = LayoutParams.MatchParent;
                        parames.Width = LayoutParams.MatchParent;
                        itemView.SetChecked(false);
                        parames.SetMargins(0, 8, 0, 8);
                    }
                }
            }
        }

        private BottomNavigationItemView GetBottomNavigationItemView(int tabIndex)
        {
            if (tabIndex < 0)
                return null;

            var menu = (BottomNavigationMenuView)_bottomNavigationView.GetChildAt(0);
            return menu.GetChildAt(tabIndex) as BottomNavigationItemView;
        }

        private void UpdateTabSelection(int itemId, Page currentpage, bool isPagedisabled)
        {
            if (TabbedPageTransforms.GetDisableSelection(currentpage))
                return;

            var lastPage = Element.GetChildPageWithTransform(selectedTabId);
            var islastpagedisabled = lastPage == null ? false : TabbedPageTransforms.GetDisableLoad(lastPage);
            var selectedtabcolor = this.Element.SelectedTabColor;
            var unselectedtabcolor = this.Element.UnselectedTabColor;

            if (_bottomNavigationView != null)
            {
                var currentTabView = GetBottomNavigationItemView(itemId);
                var lastTabView = GetBottomNavigationItemView(selectedTabId);
                if (selectedTabId == itemId)
                {
                    if (TabbedPageTransforms.GetDisableLoad(currentpage))
                    {
                        var setColor = selectedtabcolor;
                        var resetColor = unselectedtabcolor;

                        if (!isdisabledTabpressed)
                        {
                            isdisabledTabpressed = true;
                            setColor = unselectedtabcolor;
                            resetColor = selectedtabcolor;
                        }
                        else if (isdisabledTabpressed)
                            isdisabledTabpressed = false;

                        this.UpdateTabColor(currentpage, setColor, currentTabView);
                        if (lastactiveTabId >= 0)
                        {
                            var lastactivePage = Element.GetChildPageWithTransform(lastactiveTabId);
                            var lastactiveTabView = GetBottomNavigationItemView(lastactiveTabId);
                            this.UpdateTabColor(lastactivePage, resetColor, lastactiveTabView);
                        }
                    }
                }
                else if (selectedTabId != itemId && Element.Children.Count > itemId && itemId >= 0)
                {
                    if (isdisabledTabpressed && lastactiveTabId >= 0)
                    {
                        var lastactivePage = Element.GetChildPageWithTransform(lastactiveTabId);
                        var lastactiveTabView = GetBottomNavigationItemView(lastactiveTabId);
                        this.UpdateTabColor(lastactivePage, unselectedtabcolor, lastactiveTabView);
                    }

                    isdisabledTabpressed = false;
                    this.UpdateTabColor(currentpage, selectedtabcolor, currentTabView);
                    this.UpdateTabColor(lastPage, unselectedtabcolor, lastTabView);

                    if (isPagedisabled && !islastpagedisabled)
                        lastactiveTabId = selectedTabId;
                }
            }

            //For future purpose.
            //if (istitledisabled && islastpagedisabled)
            //{
            //    this.UpdateTabColor(lastPage, unselectedtabcolor, lastTabView);
            //    if (lastactiveTabId >= 0)
            //    {
            //        var lastactivePage = Element.GetChildPageWithTransform(lastactiveTabId);
            //        var lastactiveTabView = GetBottomNavigationItemView(lastactiveTabId);
            //        this.UpdateTabColor(lastactivePage, selectedtabcolor, lastactiveTabView);
            //    }
            //}
            selectedTabId = itemId;
            TabPage.IsSelectionChanged = false;
        }

        private void UpdateTabColor(Page page, Xamarin.Forms.Color color, BottomNavigationItemView itemView)
        {
            if (itemView == null)
                return;

            var nativeColor = ColorStateList.ValueOf(color.ToAndroid());
            itemView.SetIconTintList(nativeColor);
            itemView.SetTextColor(nativeColor);
        }

        private void Cleanup(Xamarin.Forms.TabbedPage page)
        {
            if (page == null)
                return;

            foreach (var tab in page.Children.Select(c => c.GetPageWithBadge()))
            {
                if (tab != null)
                    tab.PropertyChanged -= OnTabbedPagePropertyChanged;
            }

            page.ChildRemoved -= OnTabRemoved;
            page.ChildAdded -= OnTabAdded;
            BadgeViews.Clear();
            _bottomTabStrip = null;
            _bottomNavigationView = null;
        }

        private void AddTabBadge(int tabIndex)
        {
            var page = Element.GetChildPageWithBadge(tabIndex);
            if (page == null)
                return;

            var placement = Element.OnThisPlatform().GetToolbarPlacement();
            var targetView = placement == ToolbarPlacement.Bottom ? _bottomTabStrip?.GetChildAt(tabIndex) : null;
            if (!(targetView is ViewGroup targetLayout))
            {
                Console.WriteLine("Plugin.Badge: Badge target cannot be null. Badge not added.");
                return;
            }

            var badgeView = targetLayout.FindChildOfType<BadgeView>();

            if (badgeView == null)
            {
                var imageView = targetLayout.FindChildOfType<ImageView>();
                if (placement == ToolbarPlacement.Bottom)
                {
                    // create for entire tab layout
                    badgeView = BadgeView.ForTargetLayout(Context, imageView);
                }
                else
                {
                    //create badge for tab image or text
                    badgeView = BadgeView.ForTarget(Context, imageView?.Drawable != null ? (UIView)imageView : targetLayout.FindChildOfType<TextView>());
                }
            }

            BadgeViews[page] = badgeView;
            badgeView.UpdateFromElement(page);
            page.PropertyChanged -= OnTabbedPagePropertyChanged;
            page.PropertyChanged += OnTabbedPagePropertyChanged;
        }

        private void OnTabbedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is Element element))
                return;

            if (BadgeViews.TryGetValue(element, out var badgeView))
            {
                badgeView.UpdateFromPropertyChangedEvent(element, e);
            }
        }

        private void OnTabRemoved(object sender, ElementEventArgs e)
        {
            e.Element.PropertyChanged -= OnTabbedPagePropertyChanged;
            BadgeViews.Remove(e.Element);
            this.UpdateCurrentPage();
        }

        private async void OnTabAdded(object sender, ElementEventArgs e)
        {
            await Task.Delay(DeleayBeforeTabAdded);

            if (!(e.Element is Page page))
                return;

            AddTabBadge(Element.Children.IndexOf(page));
            this.UpdateCurrentPage();
        }

        #endregion
    }
}