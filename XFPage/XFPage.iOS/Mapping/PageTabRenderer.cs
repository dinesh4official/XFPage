using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreFoundation;
using CoreGraphics;
using FFImageLoading;
using FFImageLoading.Svg.Platform;
using Foundation;
using XFPage.Control;
using XFPage.iOS.Control;
using XFPage.iOS.Helper;
using XFPage.iOS.Mapping;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TabPage), typeof(PageTabRenderer))]
namespace XFPage.iOS.Mapping
{
    [Foundation.Preserve(AllMembers = true)]
    public class PageTabRenderer : TabbedRenderer
    {
        #region Fields

        private string hidepageTitle;
        private int hidepageIndex = -1;
        private bool isViewLoaded = false;
        private int selectedTabId = -1;
        private int lastactiveTabId = -1;
        private bool isdisabledTabpressed = false;

        #endregion

        #region Properties

        public TabPage MainPage { get { return this.Element as TabPage; } }

        #endregion

        #region Constructor

        public PageTabRenderer()
        {

        }

        #endregion

        #region Override Methods

        protected override void OnElementChanged(VisualElementChangedEventArgs e)
        {
            base.OnElementChanged(e);
            if (this.NativeView != null)
            {
                if (e.OldElement != null && e.OldElement is TabbedPage tabbedPage)
                    this.Cleanup(tabbedPage);

                if (e.NewElement != null)
                {
                    this.SelectedItemViewController();
                    e.NewElement.PropertyChanged += MainPage_PropertyChanged;
                }
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            this.TabItemCleanUp(Tabbed);
            this.Tabbed.ChildAdded += OnTabAdded;
            this.Tabbed.ChildRemoved += OnTabRemoved;
            this.UpdateTabBarItems();

            if (!isViewLoaded)
            {
                DispatchQueue.MainQueue.DispatchAsync(() =>
                {
                    this.UpdateItemBounds();
                    this.UpdateCurrentPage();
                    isViewLoaded = true;
                });
            }
        }

        protected override async Task<Tuple<UIImage, UIImage>> GetIcon(Page page)
        {
            var navigationPage = page as NavigationPage;
            if (navigationPage != null && navigationPage.CurrentPage != null)
            {
                var imageSource = navigationPage.IconImageSource == null ? navigationPage.CurrentPage.IconImageSource : navigationPage.IconImageSource;
                return await this.GetNativeUIImage(imageSource);
            }

            return await this.GetNativeUIImage(page.IconImageSource);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            var tabItem = this.GetTabBarItem(hidepageIndex);
            if (tabItem != null)
                this.UpdateHideTitle(tabItem);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                this.Cleanup(Tabbed);
            }
        }

        #endregion

        #region Callback Methods

        private void OnTabbedPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var page = sender as Page;
            if (page == null)
                return;

            if (e.PropertyName == Page.IconImageSourceProperty.PropertyName)
            {
                if (this.CheckValidTabIndex(page, out int tabIndex))
                {
                    this.UpdateTabBadgeText(TabBar.Items[tabIndex], page);
                    this.UpdateTabBadgeColor(TabBar.Items[tabIndex], page);
                    this.UpdateTabBadgeTextAttributes(TabBar.Items[tabIndex], page);
                }
            }
            else if (e.PropertyName == TabBadge.BadgeTextProperty.PropertyName)
            {
                if (this.CheckValidTabIndex(page, out int tabIndex))
                    this.UpdateTabBadgeText(TabBar.Items[tabIndex], page);

            }
            else if (e.PropertyName == TabBadge.BadgeColorProperty.PropertyName)
            {
                if (this.CheckValidTabIndex(page, out int tabIndex))
                    this.UpdateTabBadgeColor(TabBar.Items[tabIndex], page);

            }
            else if (e.PropertyName == TabBadge.BadgeTextColorProperty.PropertyName || e.PropertyName == TabBadge.BadgeFontProperty.PropertyName)
            {
                if (this.CheckValidTabIndex(page, out int tabIndex))
                    this.UpdateTabBadgeTextAttributes(TabBar.Items[tabIndex], page);
            }
        }

        private void MainPage_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == TabPage.IsSelectionChangedProperty.PropertyName && MainPage.IsSelectionChanged) || e.PropertyName == "CurrentPage")
            {
                this.UpdateCurrentPage();
            }
        }

        private async void OnTabAdded(object sender, ElementEventArgs e)
        {
            await Task.Delay(10);
            var page = e.Element as Page;
            if (page == null)
                return;

            var tabIndex = this.Tabbed.Children.IndexOf(page);
            this.AddTabBadge(tabIndex);
            this.UpdateCurrentPage();
            this.UpdateItemBounds();
        }

        private void OnTabRemoved(object sender, ElementEventArgs e)
        {
            e.Element.PropertyChanged -= OnTabbedPagePropertyChanged;
            this.UpdateCurrentPage();
            this.UpdateItemBounds();
        }

        #endregion

        #region Badge Implementation

        private void AddTabBadge(int tabIndex)
        {
            var element = this.Tabbed.GetChildPageWithBadge(tabIndex);
            element.PropertyChanged += OnTabbedPagePropertyChanged;
            if (this.TabBar.Items.Length > tabIndex)
            {
                var tabBarItem = this.TabBar.Items[tabIndex];
                this.UpdateTabBadgeText(tabBarItem, element);
                this.UpdateTabBadgeColor(tabBarItem, element);
                this.UpdateTabBadgeTextAttributes(tabBarItem, element);
            }
        }

        private void UpdateTabBadgeText(UITabBarItem tabBarItem, Element element)
        {
            var text = TabBadge.GetBadgeText(element);
            tabBarItem.BadgeValue = string.IsNullOrEmpty(text) ? null : text;
        }

        private void UpdateTabBadgeTextAttributes(UITabBarItem tabBarItem, Element element)
        {
            if (!tabBarItem.RespondsToSelector(new ObjCRuntime.Selector("setBadgeTextAttributes:forState:")))
            {
                Console.WriteLine("Plugin.Badge: badge text attributes only available starting with iOS 10.0.");
                return;
            }

            var attrs = new UIStringAttributes();
            var textColor = TabBadge.GetBadgeTextColor(element);

            if (textColor != Color.Default)
                attrs.ForegroundColor = textColor.ToUIColor();

            var font = TabBadge.GetBadgeFont(element);
            if (font != Font.Default)
                attrs.Font = font.ToUIFont();

            tabBarItem.SetBadgeTextAttributes(attrs, UIControlState.Normal);
        }

        private void UpdateTabBadgeColor(UITabBarItem tabBarItem, Element element)
        {
            if (!tabBarItem.RespondsToSelector(new ObjCRuntime.Selector("setBadgeColor:")))
            {
                Console.WriteLine("Plugin.Badge: badge color only available starting with iOS 10.0.");
                return;
            }

            var tabColor = TabBadge.GetBadgeColor(element);
            if (tabColor != Color.Default)
                tabBarItem.BadgeColor = tabColor.ToUIColor();
        }

        private bool CheckValidTabIndex(Page page, out int tabIndex)
        {
            tabIndex = Tabbed.Children.IndexOf(page);

            if (tabIndex == -1 && page.Parent != null)
                tabIndex = this.Tabbed.Children.IndexOf(page.Parent);

            return tabIndex >= 0 && tabIndex < TabBar.Items.Length;
        }

        private void UpdateBadgeView(Page page, UIView view, float position)
        {
            var badgeView = view.Subviews[2];
            if (badgeView is NativeCircleView)
                return;

            var xvalue = (position / 2) + 14;
            badgeView.RemoveFromSuperview();
            var color = TabBadge.GetBadgeColor(page).ToUIColor();
            var circleView = new NativeCircleView();
            circleView.IsActive = true;
            circleView.FillColor = color;
            circleView.StrokeColor = color;
            circleView.Frame = new CGRect(xvalue, 4, 7, 7);
            view.AddSubview(circleView);
        }

        #endregion

        #region Private Methods

        private void Cleanup(TabbedPage tabbedPage)
        {
            if (tabbedPage == null)
                return;

            tabbedPage.PropertyChanged -= MainPage_PropertyChanged;
            this.TabItemCleanUp(tabbedPage);
        }

        private void TabItemCleanUp(TabbedPage tabbedPage)
        {
            foreach (var tab in tabbedPage.Children.Select(c => c.GetPageWithBadge()))
                tab.PropertyChanged -= OnTabbedPagePropertyChanged;

            tabbedPage.ChildAdded -= OnTabAdded;
            tabbedPage.ChildRemoved -= OnTabRemoved;
        }

        private void UpdateCurrentPage()
        {
            var page = this.Tabbed.CurrentPage;
            var itemId = this.Tabbed.Children.IndexOf(page);
            this.UpdateTabSelection(itemId, page, TabbedPageTransforms.GetDisableLoad(page));
        }

        private void UpdateTabBarItems()
        {
            try
            {
                for (int i = 0; i < this.TabBar.Items.Length; i++)
                {
                    var page = this.Tabbed.GetChildPageWithTransform(i);
                    var tabItem = this.TabBar.Items[i];
                    page.PropertyChanged += OnTabbedPagePropertyChanged;
                    if (TabbedPageTransforms.GetHideTitle(page))
                    {
                        this.hidepageIndex = i;
                        this.hidepageTitle = page.Title;
                        this.UpdateHideTitle(tabItem);
                    }

                    this.UpdateTabBadgeText(tabItem, page);
                    this.UpdateTabBadgeColor(tabItem, page);
                    this.UpdateTabBadgeTextAttributes(tabItem, page);

                    if (!string.IsNullOrEmpty(TabBadge.GetBadgeText(page)))
                    {
                        var view = this.GetTabItemView(tabItem.Title, true);
                        if (view != null)
                            this.UpdateBadgeView(page, view, (float)view.Frame.Width);
                    }
                }
            }
            catch
            {

            }
        }

        private async Task<Tuple<UIImage, UIImage>> GetNativeUIImage(ImageSource imageSource)
        {
            var imageicon = await GetNativeImageAsync(imageSource);
            return new Tuple<UIImage, UIImage>(imageicon, null);
        }

        private async Task<UIImage> GetNativeImageAsync(ImageSource imageSource)
        {
            if (imageSource is FileImageSource fileImage && fileImage.File.Contains(".svg"))
            {
                var imageicon = await ImageService.Instance.LoadFile(fileImage.File).WithCustomDataResolver(new SvgDataResolver(15, 15, true)).AsUIImageAsync();
                return imageicon.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            }
            else if (imageSource is FontImageSource fontImage)
            {
                var imageicon = XamarinExtensions.ConvertFontIconToUIImage(fontImage.FontFamily, fontImage.Size, UIColor.White, fontImage.Glyph);
                return imageicon.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            }
            else
            {
                var imageicon = await XamarinExtensions.GetUIImage(imageSource);
                return imageicon.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            }
        }

        private void SelectedItemViewController()
        {
            ShouldSelectViewController = (tabController, controller) =>
            {
                var index = tabController.ViewControllers?.IndexOf(controller);
                if (index.HasValue)
                {
                    var tabnavigationresult = TabbedPageTransforms.TabIconClickedFunction(index.Value, selectedTabId, this);
                    if (tabnavigationresult)
                        return false;

                    var page = this.Tabbed.GetChildPageWithTransform(index.Value);
                    var isdisabledPage = TabbedPageTransforms.GetDisableLoad(page);
                    this.UpdateTabSelection(index.Value, page, isdisabledPage);
                    return !isdisabledPage;
                }

                return true;
            };
        }

        private void UpdateTabSelection(int itemId, Page currentpage, bool isPagedisabled)
        {
            if (this.TabBar == null || this.TabBar.Items == null || this.TabBar.Items.Count() <= 0 || TabbedPageTransforms.GetDisableSelection(currentpage))
                return;

            var lastPage = this.Tabbed.GetChildPageWithTransform(selectedTabId);
            var islastpagedisabled = lastPage == null ? false : TabbedPageTransforms.GetDisableLoad(lastPage);
            var selectedtabcolor = this.Tabbed.SelectedTabColor;
            var unselectedtabcolor = this.Tabbed.UnselectedTabColor;

            var currentTabView = this.GetTabBarItem(itemId);
            var lastTabView = this.GetTabBarItem(selectedTabId);
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
                        var lastactivePage = Tabbed.GetChildPageWithTransform(lastactiveTabId);
                        var lastactiveTabView = this.GetTabBarItem(lastactiveTabId);
                        this.UpdateTabColor(lastactivePage, resetColor, lastactiveTabView);
                    }
                }
            }
            else if (selectedTabId != itemId && itemId >= 0)
            {
                if (isdisabledTabpressed && lastactiveTabId >= 0)
                {
                    var lastactivePage = Tabbed.GetChildPageWithTransform(lastactiveTabId);
                    var lastactiveTabView = this.GetTabBarItem(lastactiveTabId);
                    this.UpdateTabColor(lastactivePage, unselectedtabcolor, lastactiveTabView);
                }

                isdisabledTabpressed = false;
                this.UpdateTabColor(currentpage, selectedtabcolor, currentTabView);
                this.UpdateTabColor(lastPage, unselectedtabcolor, lastTabView);

                if (isPagedisabled && !islastpagedisabled)
                    lastactiveTabId = selectedTabId;
            }

            selectedTabId = itemId;
            this.MainPage.IsSelectionChanged = false;
        }

        private void UpdateTabColor(Page page, Xamarin.Forms.Color color, UITabBarItem itemView)
        {
            if (itemView == null)
                return;

            itemView.SetTitleTextAttributes(new UITextAttributes() { TextColor = color.ToUIColor() }, UIControlState.Normal);
        }

        private UITabBarItem GetTabBarItem(int index)
        {
            return index < 0 ? null : this.TabBar.Items[index];
        }

        private void UpdateItemBounds()
        {
            var tabbarFrame = this.TabBar.Frame;
            var rectangle = new Rectangle();
            rectangle.Y = tabbarFrame.Y;
            rectangle.Height = tabbarFrame.Y + tabbarFrame.Height;
            double tempHeight = 0;
            for (int i = 0; i < this.TabBar.Items.Count(); i++)
            {
                string title = this.TabBar.Items[i].Title;
                title = !string.IsNullOrEmpty(title) ? title : hidepageTitle;
                var frame = this.GetSubViewFrame(title);

                if (frame.Height != 0)
                    tempHeight = frame.Height + frame.Y;

                rectangle.X = frame.X;
                rectangle.Width = frame.Width;

                if (this.MainPage.TabItemBounds.ContainsKey(title))
                    this.MainPage.TabItemBounds.Remove(title);

                this.MainPage.TabItemBounds.Add(title, rectangle);
            }

            this.MainPage.BottomBarHeight = tempHeight;
        }

        private CGRect GetSubViewFrame(string title)
        {
            for (int i = 0; i < this.TabBar.Subviews.Count(); i++)
            {
                var subView = this.TabBar.Subviews[i];
                UILabel label = this.GetUILabel(subView);
                if (label != null && label.Text == title)
                    return subView.Frame;
            }

            return new CGRect();
        }

        private UIView GetTabItemView(string title, bool isBadgeView)
        {
            for (int i = 0; i < this.TabBar.Subviews.Count(); i++)
            {
                var subView = this.TabBar.Subviews[i];
                if (isBadgeView)
                {
                    if (subView.Subviews.Count() == 3)
                        return subView;
                }
                else
                {
                    UILabel label = this.GetUILabel(subView);
                    if (label != null && label.Text == title)
                        return subView;
                }
            }

            return null;
        }

        private UILabel GetUILabel(UIView uiview)
        {
            if (uiview is UIControl control)
            {
                for (int i = 0; i < control.Subviews.Count(); i++)
                {
                    var view = control.Subviews[i] as UILabel;
                    if (view != null)
                        return view;
                }
            }

            return null;
        }

        private void UpdateHideTitle(UITabBarItem tabItem)
        {
            tabItem.Title = string.Empty;
            tabItem.SetTitleTextAttributes(new UITextAttributes() { TextColor = UIColor.Clear }, UIControlState.Normal);
            tabItem.ImageInsets = new UIEdgeInsets(2, 0, 2, 0);
        }

        #endregion
    }
}