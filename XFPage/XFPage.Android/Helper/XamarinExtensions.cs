using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Support.Design.Widget;
using Android.Support.Design.Internal;
using XFPage.Control;
using XFPage.Droid.Control;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using ChildView = Android.Views.View;
using Color = Xamarin.Forms.Color;
using NativeView = Android.Views.ViewGroup;
 
namespace XFPage.Droid.Helper
{
    public static class XamarinExtensions
    {
        #region Static Methods

        internal static async Task<Drawable> GetFormsDrawableAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageSource == null || imageSource.IsEmpty)
                return null;

            using (var bitmap = await context.GetFormsBitmapAsync(imageSource, cancellationToken).ConfigureAwait(false))
            {
                if (bitmap != null)
                    return new BitmapDrawable(context.Resources, bitmap);
            }

            return null;
        }

        internal static async Task<Bitmap> GetFormsBitmapAsync(this Context context, ImageSource imageSource, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (imageSource == null || imageSource.IsEmpty)
                return null;

            var handler = Registrar.Registered.GetHandlerForObject<IImageSourceHandler>(imageSource);
            if (handler == null)
                return null;

            try
            {
                return await handler.LoadImageAsync(imageSource, context, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("Image loading", "Image load cancelled");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Image loading", $"Image load failed: {ex}");
            }

            return null;
        }

        public static NativeView GetModalContainer(this IViewParent parent)
        {
            if (parent is NativeView ParentView)
            {
                if (ParentView.Class.Name.Contains("ModalContainer"))
                    return ParentView;
                else if (ParentView.Parent != null)
                    return ParentView.Parent.GetModalContainer();
            }

            return null;
        }

        public static T Cast<T>(Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        public static T FindChildOfType<T>(this NativeView parent) where T : ChildView
        {
            if (parent == null)
                return null;

            if (parent.ChildCount == 0)
                return null;

            for (var i = 0; i < parent.ChildCount; i++)
            {
                var child = parent.GetChildAt(i);


                if (child is T typedChild)
                {
                    return typedChild;
                }

                if (!(child is NativeView))
                    continue;


                var result = FindChildOfType<T>(child as NativeView);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static void BottomNavigationSetShiftMode(this BottomNavigationView bottomNavigationView, bool enableShiftMode, bool enableItemShiftMode)
        {
            try
            {
                var menuView = bottomNavigationView.GetChildAt(0) as BottomNavigationMenuView;
                if (menuView == null)
                {
                    System.Diagnostics.Debug.WriteLine("Unable to find BottomNavigationMenuView");
                    return;
                }


                var shiftMode = menuView.Class.GetDeclaredField("mShiftingMode");

                shiftMode.Accessible = true;
                shiftMode.SetBoolean(menuView, enableShiftMode);
                shiftMode.Accessible = false;
                shiftMode.Dispose();


                for (int i = 0; i < menuView.ChildCount; i++)
                {
                    var item = menuView.GetChildAt(i) as BottomNavigationItemView;
                    if (item == null)
                        continue;

                    item.SetShifting(enableItemShiftMode);
                    item.SetChecked(item.ItemData.IsChecked);

                }

                menuView.UpdateMenuView();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unable to set shift mode: {ex}");
            }
        }

        public static void UpdateFromElement(this BadgeView badgeView, Page element)
        {
            var badgeText = TabBadge.GetBadgeText(element);
            badgeView.Text = badgeText;

            var tabColor = TabBadge.GetBadgeColor(element);
            if (tabColor != Color.Default)
            {
                badgeView.BadgeColor = tabColor.ToAndroid();
            }

            var tabTextColor = TabBadge.GetBadgeTextColor(element);
            if (tabTextColor != Color.Default)
            {
                badgeView.TextColor = tabTextColor.ToAndroid();
            }

            // set font if not default
            var font = TabBadge.GetBadgeFont(element);
            if (font != Font.Default)
            {
                badgeView.Typeface = font.ToTypeface();
            }

            var margin = TabBadge.GetBadgeMargin(element);
            badgeView.SetMargins((float)margin.Left, (float)margin.Top, (float)margin.Right, (float)margin.Bottom);
            badgeView.Postion = TabBadge.GetBadgePosition(element);
        }

        public static void UpdateFromPropertyChangedEvent(this BadgeView badgeView, Element element, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == TabBadge.BadgeTextProperty.PropertyName)
            {
                badgeView.Text = TabBadge.GetBadgeText(element);
            }
            else if (e.PropertyName == TabBadge.BadgeColorProperty.PropertyName)
            {
                badgeView.BadgeColor = TabBadge.GetBadgeColor(element).ToAndroid();
            }
            else if (e.PropertyName == TabBadge.BadgeTextColorProperty.PropertyName)
            {
                badgeView.TextColor = TabBadge.GetBadgeTextColor(element).ToAndroid();
            }
            else if (e.PropertyName == TabBadge.BadgeFontProperty.PropertyName)
            {
                badgeView.Typeface = TabBadge.GetBadgeFont(element).ToTypeface();
            }
            else if (e.PropertyName == TabBadge.BadgePositionProperty.PropertyName)
            {
                badgeView.Postion = TabBadge.GetBadgePosition(element);
            }
            else if (e.PropertyName == TabBadge.BadgeMarginProperty.PropertyName)
            {
                var margin = TabBadge.GetBadgeMargin(element);
                badgeView.SetMargins((float)margin.Left, (float)margin.Top, (float)margin.Right, (float)margin.Bottom);
            }
        }

        #endregion
    }
}