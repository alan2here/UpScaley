// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
    public struct RGB
    {
        public byte r, g, b;

        public RGB(byte r, byte g, byte b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public RGB(System.Drawing.Color c)
        {
            r = c.R;
            g = c.G;
            b = c.B;
        }

        public System.Drawing.Color to_Color()
        {
            return System.Drawing.Color.FromArgb(r, g, b);
        }

        public float shade
        {
            get
            {
                return (r + g + b) / 3f;
            }
        }

        public byte this[byte c]
        {
            get {
                if (c == 0)
                    return r;
                else if (c == 1)
                    return g;
                else if (c == 2)
                    return b;
                else
                    throw new ArgumentException();
                // return c == 0 ?
                //     r :
                //     (c == 1 ? g : (c == 2 ? b : throw new ArgumentException()));
            }

            set {
                if (c == 0)
                    r = value;
                else if (c == 1)
                    g = value;
                else if (c == 2)
                    b = value;
                else throw new ArgumentException();
            }
        }

        public byte[] store()
        {
            return new []{r, g, b};
        }
    }
}