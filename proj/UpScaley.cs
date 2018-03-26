// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
    public class Program
    {
        static void Main(string[] args)
        {
            writer.write("reading args");
            var arg_count = args.Length;
            string filename;
            if (arg_count > 0)
                filename = args[0];
            else
                throw new ArgumentException("Filename required as command-line argument.");
            UInt16? scoring_function_index = null;
            byte? scale = null, replacement_size_ish = null;
            if (arg_count > 1) {
                if (args[1] != "_")
                    scoring_function_index = Convert.ToUInt16(args[1]);
                if (arg_count > 2) {
                    if (args[2] != "_")
                        scale = Convert.ToByte(args[2]);
                    if (arg_count > 3) {
                        if (arg_count == 4)
                        {
                            if (args[3] != "_")
                                replacement_size_ish = Convert.ToByte(args[3]);
                        }
                        else
                            throw new ArgumentException("too many arguments, 4 max");
                    }
                }
            }
            writer.write("precomputing");
            init();
            var source = "../../../pics/";
            writer.write("loading pic");
            var pic = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(source + filename));
            var pic2 = new img(pic);
            var result = upscale(pic2, scoring_function_index, scale, replacement_size_ish, 4).to_bitmap();
            writer.write("saving result");
            result.Save(source + "result.png", System.Drawing.Imaging.ImageFormat.Png);
            writer.write("clearing up");
            pic.Dispose();
            result.Dispose();
            writer.write("done");
            Console.ReadLine();
        }

        private static void init()
        {
            UInt16 a, b, c;
            tables.recip_upTo769[0] = 4;
            for (a = 1; a < 770; a++)
                tables.recip_upTo769[a] = 1f / a;
            for (a = 0; a < 770; a++)
            {
                tables.recip_upTo769_offset1[a] = 1f / (a + 1);
                tables.recip_upTo769_offset5[a] = 1f / (a + 5);
                tables.recip_upTo769_offset20[a] = 1f / (a + 20);
                tables.recip_upTo769_offset100[a] = 1f / (a + 100);
            }
            for (a = 0; a < 256; a++)
                for (b = 0; b < 256; b++)
                {
                    tables.abs_dif[a, b] = (byte)((a - b) < 0 ? -(a - b) : (a - b));
                    tables.abs_dif_retInt16[a, b] = (Int16)((a - b) < 0 ? -(a - b) : (a - b));
                    for (c = 0; c < 256; c++)
                        tables.sqrt_sum[a, b, c] = (float)Math.Sqrt(a + b + c);
                }
            UInt32 d, e, f;
            for (d = 0; d < 256; d++)
                for (e = 0; e < 256; e++)
                    for (f = 0; f < 256; f++)
                        tables.sqrt_sum_sqrs[d, e, f] = (float)Math.Sqrt((d * d) + (e * e) + (f * f));
        }

        // cannot process close to the edge yet
        private static img upscale(
            img pic, UInt16? scoring_function_index = null,
            byte? scale = null, byte? replacement_size_ish = null,
            byte? verbosity = null)
        {
            if (verbosity == null)
                verbosity = 3;
            if (verbosity > 0)
                writer.write_then_inc("upscale");
            if (scoring_function_index == null)
                scoring_function_index = 0;
            if (scale == null)
                scale = 5;
            if (replacement_size_ish == null)
                replacement_size_ish = 3;
            if (verbosity > 1)
                writer.write("scoring function index: " + scoring_function_index);
            var scoring_function = UpScaley.score.get((UInt16)scoring_function_index);
            if (verbosity > 1) {
                writer.write("image width: " + pic.RGB.GetLength(0));
                writer.write("image height: " + pic.RGB.GetLength(1));
                writer.write("Scale base image by " + scale + " times.");
            }
            if (scale == 0)
                throw new ArgumentException("Scale cannot be 0.");
            var Base = expand.bicubic(pic, (float)scale);
            var replacement_size = (UInt16)(scale * replacement_size_ish);
            if (verbosity > 1) {
                writer.write("base width: " + Base.RGB.GetLength(0));
                writer.write("base height: " + Base.RGB.GetLength(1));
                writer.write("style width: " + pic.RGB.GetLength(0));
                writer.write("style height: " + pic.RGB.GetLength(1));
                writer.write("replacement size: " + scale + " * " + replacement_size_ish + " = " + replacement_size);
            }
            var result = style_transfer.trans(Base, pic, scoring_function, replacement_size, (byte)verbosity);
            if (verbosity > 0)
                writer.dec();
            return result;
        }
    }
}