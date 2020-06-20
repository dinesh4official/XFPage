using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using ChildView = UIKit.UIView;
using Color = Xamarin.Forms.Color;
using DrawRect = System.Drawing.RectangleF;
using NativeView = UIKit.UIView;
using PlatFormHelper = Xamarin.Forms.Platform.iOS.Platform;

namespace XFPage.iOS.Helper 
{
    [Preserve(AllMembers = true)]
    internal static partial class XamarinExtensions
    {
        public static Task<UIImage> GetUIImage(ImageSource imageSource)
        {
            var handler = GetImageSourceHandler(imageSource);
            return handler.LoadImageAsync(imageSource);
        }

        public static IImageSourceHandler GetImageSourceHandler(ImageSource source)
        {
            IImageSourceHandler sourceHandler = null;
            if (source is UriImageSource)
                sourceHandler = new ImageLoaderSourceHandler();
            else if (source is FileImageSource)
                sourceHandler = new FileImageSourceHandler();
            else if (source is StreamImageSource)
                sourceHandler = new StreamImagesourceHandler();
            else if (source is FontImageSource)
                sourceHandler = new FontImageSourceHandler();

            return sourceHandler;
        }

        public static UIImage ConvertFontIconToUIImage(string fontFamily, double fontSize, UIColor iconcolor, string glyph)
        {
            UIImage image = null;
            var cleansedname = CleanseFontName(fontFamily);
            var font = UIFont.FromName(cleansedname ?? string.Empty, (float)fontSize) ?? UIFont.SystemFontOfSize((float)fontSize);
            var attString = new NSAttributedString(glyph, font: font, foregroundColor: iconcolor);
            var imagesize = ((NSString)glyph).GetSizeUsingAttributes(attString.GetUIKitAttributes(0, out _));
            UIGraphics.BeginImageContextWithOptions(imagesize, false, 0f);
            var ctx = new NSStringDrawingContext();
            var boundingRect = attString.GetBoundingRect(imagesize, 0, ctx);
            attString.DrawString(new DrawRect((float)(imagesize.Width / 2 - boundingRect.Size.Width / 2), (float)(imagesize.Height / 2 - boundingRect.Size.Height / 2), (float)imagesize.Width, (float)imagesize.Height));
            image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        internal static string CleanseFontName(string fontName)
        {
            var (hasFontAlias, fontPostScriptName) = Xamarin.Forms.Internals.FontRegistrar.HasFont(fontName);
            if (hasFontAlias)
                return fontPostScriptName;

            var fontFile = FontFile.FromString(fontName);

            if (!string.IsNullOrWhiteSpace(fontFile.Extension))
            {
                var (hasFont, filePath) = Xamarin.Forms.Internals.FontRegistrar.HasFont(fontFile.FileNameWithExtension());
                if (hasFont)
                    return filePath ?? fontFile.PostScriptName;
            }
            else
            {
                foreach (var ext in FontFile.Extensions)
                {

                    var formated = fontFile.FileNameWithExtension(ext);
                    var (hasFont, filePath) = Xamarin.Forms.Internals.FontRegistrar.HasFont(formated);
                    if (hasFont)
                        return filePath;
                }
            }

            return fontFile.PostScriptName;
        }
    }
}