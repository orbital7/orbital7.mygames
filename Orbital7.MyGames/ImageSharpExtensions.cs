using ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageSharp
{
    // TODO: Move this class into Orbital7.Extensions after .NET Standard 2.0 and availably of ImageSharp package.
    public static class ImageSharpExtensions
    {
        public static Image<Color, uint> EnsureMaximumSize(this Image image, int maxWidth, int maxHeight, 
            bool maintainAspectRatio = true)
        {
            if ((image.Width > maxWidth) || (image.Height > maxHeight))
            {
                if (maintainAspectRatio)
                {
                    Image<Color, uint> sizedBitmap = image;

                    // Handle if width is larger.
                    if (image.Width > image.Height)
                        sizedBitmap = image.Resize(maxWidth, Convert.ToInt32(image.Height * maxWidth / image.Width));
                    // Else height is larger.
                    else
                        sizedBitmap = image.Resize(Convert.ToInt32(image.Width * maxHeight / image.Height), maxHeight);

                    return sizedBitmap;
                }
                else
                {
                    return image.Resize(maxWidth, maxHeight);
                }
            }
            else
            {
                return image;
            }
        }
    }
}
