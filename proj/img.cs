// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
    public class img
    {
        public RGB[,] RGB;

        public img() {}

        public img(Int32 width, Int32 height)
        {
            RGB = new RGB[width, height];
            Int32 x, y;
            for (x = 0; x < width; x++)
            for (y = 0; y < height; y++)
                RGB[x, y] = new RGB();
        }

        public img(System.Drawing.Bitmap b)
        {
            Int32 x, y, width = b.Width, height = b.Height;
            RGB = new RGB[width, height];
            for (x = 0; x < width; x++)
            for (y = 0; y < height; y++)
                RGB[x, y] = new RGB(b.GetPixel(x, y));
        }

        public RGB this[Int32 x, Int32 y]
        {
            get { return RGB[x, y]; }
            set { RGB[x, y] = value; }
        }

        public System.Drawing.Bitmap to_bitmap()
        {
            Int32 x, y, result_width = RGB.GetLength(0), result_height = RGB.GetLength(1);
            var result = new System.Drawing.Bitmap(result_width, result_height);
            for (x = 0; x < result_width; x++)
            for (y = 0; y < result_height; y++)
                result.SetPixel(x, y, RGB[x, y].to_Color());
            return result;
        }
    }
}