using ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Orbital7.MyGames
{
    // TODO: Move this class into Orbital7.Extensions after .NET Standard 2.0 and availably of ImageSharp package.
    public static class ImageSharpHelper
    {
        public static IImageFormat GetImageFormat(string fileExtension)
        {
            switch (fileExtension.ToLower())
            {
                case ".gif":
                    return new GifFormat();

                case ".jpg":
                case ".jpeg":
                    return new JpegFormat();

                case ".png":
                    return new PngFormat();

                case ".bmp":
                    return new BmpFormat();

                    //case ".tif":
                    //case ".tiff":
            }

            throw new Exception("The specified file extension is not supported");
        }
    }
}
