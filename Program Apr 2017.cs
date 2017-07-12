using System;

namespace style
{
    class Program
    {
        private static float[,,] sqrt_sum = new float[256, 256, 256];
        private static byte[,] abs_dif = new byte[256, 256];
        private static Int16[,] abs_dif_retInt16 = new Int16[256, 256];
        private static float[,,] sqrt_sum_sqrs = new float[256, 256, 256];
        private static float[] recip_upTo769 = new float[770];
        private static float[] recip_upTo769_offset1 = new float[770];
        private static float[] recip_upTo769_offset5 = new float[770];
        private static float[] recip_upTo769_offset20 = new float[770];
        private static byte indent = 0;

        static void Main(string[] args)
        {
            write2("precomputing");
            init();
            bool both_orig = false;
            var source = "..\\..\\pics\\";
            write2("loading orig");
            var orig = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(source + "orig.png"));
            write2("formatting orig");
            var orig2 = new img(orig);
            System.Drawing.Bitmap style = null;
            img style2 = null;
            if (both_orig)
                style2 = new img(orig);
            else
            {
                write2("loading style");
                style = new System.Drawing.Bitmap(System.Drawing.Image.FromFile(source + "style.png"));
                write2("formatting style");
                style2 = new img(style);
            }
            var result = resolve2(orig2, style2, 9, score_col_sim_inc5).to_bitmap();
            write2("saving result");
            result.Save(source + "result.png", System.Drawing.Imaging.ImageFormat.Png);
            write2("clearing up");
            orig.Dispose();
            if (!both_orig)
                style.Dispose();
            result.Dispose();
            Console.WriteLine("done");
        }

        private static void init()
        {
            UInt16 a, b, c;
            recip_upTo769[0] = 4;
            for (a = 1; a < 770; a++)
                recip_upTo769[a] = 1f / a;
            for (a = 0; a < 770; a++)
            {
                recip_upTo769_offset1[a] = 1f / (a + 1);
                recip_upTo769_offset5[a] = 1f / (a + 5);
                recip_upTo769_offset20[a] = 1f / (a + 20);
            }
            for (a = 0; a < 256; a++)
                for (b = 0; b < 256; b++)
                {
                    abs_dif[a, b] = (byte)((a - b) < 0 ? -(a - b) : (a - b));
                    abs_dif_retInt16[a, b] = (Int16)((a - b) < 0 ? -(a - b) : (a - b));
                    for (c = 0; c < 256; c++)
                        sqrt_sum[a, b, c] = (float)Math.Sqrt(a + b + c);
                }
            UInt32 d, e, f;
            for (d = 0; d < 256; d++)
                for (e = 0; e < 256; e++)
                    for (f = 0; f < 256; f++)
                        sqrt_sum_sqrs[d, e, f] = (float)Math.Sqrt((d * d) + (e * e) + (f * f));
        }

        // crop "orig"
        // repeat "passes" times
        //   resize (width & height) of "orig" by "scale" times.
        //   Resolve "result" from ("orig" & "style") also using (the "score" function, "replacement size" & "skip").
        //   replace "style" with "result"
        // yeild result
        private static img resolve2(
            // params for "resolve"
            img orig, img style, UInt16 replacement_size,
            Func<img, img, Int32, Int32, Int32, Int32, Int32, float> score,
            UInt16 skip = 1, byte verbosity = 3,

            // preparing "orig"
            UInt16 ? crop_x = null, UInt16? crop_y = null, UInt16? crop_size = null,

            // pass concept
            byte passes = 1, float? scale = null)
        {
            if (verbosity > 0)
                write("resolve2");
            if (scale <= 0f)
                throw new ArgumentException("Scale must be greater than 0.");
            if (passes == 0)
                throw new ArgumentException("At least one pass is required.");
            byte crop_use = (byte)((crop_x == null ? 0 : 1) + (crop_y == null ? 0 : 1) + (crop_size == null ? 0 : 1));
            if (crop_use == 1 || crop_use == 2)
                throw new ArgumentException("Use all crop parameters or leave all as null.");
            if (crop_use != 0)
            {
                if (verbosity > 1)
                {
                    write("crop orig");
                    write2(crop_x + ", " + crop_y + ", " + crop_size);
                    write3();
                }
                orig = crop(orig, (UInt16)crop_x, (UInt16)crop_y, (UInt16)crop_size);
            }
            img result = new img();
            byte pass;
            for (pass = 0; pass < passes; pass++)
            {
                if (verbosity > 0)
                    write2("Pass " + (pass + 1) + " of " + passes + ".");
                if (scale != null)
                {
                    if (verbosity > 1)
                        write2("Scale orig by " + scale + " times.");
                    orig = expand(orig, (float)scale);
                }
                result = resolve(orig, style, replacement_size, score, skip, verbosity);
                style = result;
            }
            if (verbosity > 0)
                write3();
            return result;
        }

        // Instead of just using the best region,
        // could try a mean average of the center pixel from:
            // the 8 best matches
            // all matches closer than 1% deviation from the best match
        private static img resolve(img orig, img style, UInt16 replacement_size,
            Func<img, img, Int32, Int32, Int32, Int32, Int32, float> score, UInt16 skip = 1,
            byte verbosity = 3)
        {
            if (verbosity > 4)
                throw new ArgumentException("4 is the maximum verbosity");
            if (score == null)
                throw new ArgumentException("A scoring function is required.");
            Int32 replacement_size2 = replacement_size;
            if (replacement_size2 % 2 == 0)
                throw new ArgumentException("replacement_size must be an odd number");
            if (skip == 0)
                throw new ArgumentException("Chance may not be 0.");
            float best_result_deviation, deviation;
            UInt32 denominator = (UInt32)(verbosity > 3 ? 10000 : 1000);
            byte numerator = 1, least_denominator = 20, cycle_pos = 0;
            Int32 orig_x, orig_y,
                orig_and_result_width = orig.RGB.GetLength(0), orig_and_result_height = orig.RGB.GetLength(1),
                style_width = style.RGB.GetLength(0), style_height = style.RGB.GetLength(1),
                orig_and_result_width_subtract_replacment_size = orig_and_result_width - replacement_size2,
                orig_and_result_height_subtract_replacment_size = orig_and_result_height - replacement_size2,
                style_width_subtract_replacment_size = style_width - replacement_size2,
                style_height_subtract_replacment_size = style_height - replacement_size2,
                best_result_x, best_result_y, half_replacement_size = (Int32)(replacement_size2 * 0.5f);
            UInt64 pixel = 0, expected_pixels = (UInt64)(orig_and_result_width_subtract_replacment_size *
                orig_and_result_height_subtract_replacment_size);
            var result = new img(orig_and_result_width, orig_and_result_height);
            if (verbosity > 0)
                write("resolve");
            if (verbosity > 1)
            {
                write2("replacement_size: " + replacement_size);
                write2("skip: " + skip);
                var region_comparisons = (UInt64)orig_and_result_width_subtract_replacment_size *
                    (UInt64)orig_and_result_height_subtract_replacment_size *
                    (UInt64)style_width_subtract_replacment_size *
                    (UInt64)style_height_subtract_replacment_size;
                write2("The load is about (" +
                    (UInt64)Math.Round(region_comparisons * (UInt64)replacement_size * (UInt64)replacement_size * 0.000000001f) +
                    " billion pixel pair comparisons) or (" + (UInt64)Math.Round(region_comparisons * 0.000001f) +
                    " million region comparisons).");
            }
            if (verbosity > 2)
            {
                write2("orig_and_result_width: " + orig_and_result_width);
                write2("orig_and_result_height: " + orig_and_result_height);
                write2("style_width: " + style_width);
                write2("style_height: " + style_height);
                write2("orig_and_result_width_subtract_replacment_size: " + orig_and_result_width_subtract_replacment_size);
                write2("orig_and_result_height_subtract_replacment_size: " + orig_and_result_height_subtract_replacment_size);
                write2("style_width_subtract_replacment_size: " + style_width_subtract_replacment_size);
                write2("style_height_subtract_replacment_size: " + style_height_subtract_replacment_size);
            }
            for (orig_x = 0; orig_x < orig_and_result_width_subtract_replacment_size; orig_x++)
            {
                for (orig_y = 0; orig_y < orig_and_result_height_subtract_replacment_size; orig_y++)
                {
                    if (verbosity > 1)
                        while ((float)pixel / expected_pixels > (float)numerator / denominator)
                        {
                            write2("" + numerator + " / " + denominator);
                            if (denominator == least_denominator)
                                numerator++;
                            else
                                denominator = (UInt32)(denominator * (cycle_pos % 3 == 1 ? 0.4f : 0.5f));
                            cycle_pos++;
                        }
                    pixel++;
                    best_result_x = 0;
                    best_result_y = 0;
                    best_result_deviation = Int32.MaxValue;
                    System.Threading.Tasks.Parallel.For(0, style_width_subtract_replacment_size, style_x =>
                    {
                        for (Int32 style_y = 0; style_y < style_height_subtract_replacment_size; style_y++)
                        {
                            if ((style_x + (style_width_subtract_replacment_size * style_y)) % skip == 0)
                            {
                                deviation = score(orig, style, orig_x, orig_y, style_x, style_y, replacement_size2);
                                if (deviation < best_result_deviation)
                                {
                                    best_result_x = style_x;
                                    best_result_y = style_y;
                                    best_result_deviation = deviation;
                                }
                            }
                        }
                    });
                    result[orig_x + half_replacement_size, orig_y + half_replacement_size] =
                        style[best_result_x + half_replacement_size, best_result_y + half_replacement_size];
                }
            }
            if (verbosity > 0)
            {
                write2("Pixels of orig/result calculated: " + pixel);
                write3();
            }
            return result;
        }

        private static float score_col_sim_inc1(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation -= recip_upTo769_offset1[abs_dif_retInt16[a2.r, b2.r] + abs_dif_retInt16[a2.g, b2.g] + abs_dif_retInt16[a2.b, b2.b]];
                }
            return deviation;
        }

        // perhaps the best
        private static float score_col_sim_inc5(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation -= recip_upTo769_offset5[abs_dif_retInt16[a2.r, b2.r] + abs_dif_retInt16[a2.g, b2.g] + abs_dif_retInt16[a2.b, b2.b]];
                }
            return deviation;
        }

        private static float score_col_sim_inc20(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation -= recip_upTo769_offset20[abs_dif_retInt16[a2.r, b2.r] + abs_dif_retInt16[a2.g, b2.g] + abs_dif_retInt16[a2.b, b2.b]];
                }
            return deviation;
        }

        private static float score_col_sim(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation -= recip_upTo769[abs_dif_retInt16[a2.r, b2.r] + abs_dif_retInt16[a2.g, b2.g] + abs_dif_retInt16[a2.b, b2.b]];
                }
            return deviation;
        }

        private static float score_col_dist(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation += sqrt_sum_sqrs[abs_dif[a2.r, b2.r], abs_dif[a2.g, b2.g], abs_dif[a2.b, b2.b]];
                }
            return deviation;
        }

        private static float score_col(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            RGB a2, b2;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    a2 = a[region_a_left + x, region_a_top + y];
                    b2 = b[region_b_left + x, region_b_top + y];
                    deviation += abs_dif[a2.r, b2.r] + abs_dif[a2.g, b2.g] + abs_dif[a2.b, b2.b];
                }
            return deviation;
        }

        // not so good
        private static float score_grey(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                    deviation += (byte)Math.Abs(a[region_a_left + x, region_a_top + y].shade -
                        b[region_b_left + x, region_b_top + y].shade);
            return deviation;
        }

        // not so good
        private static float score_grey_normal(
            img a, img b,
            Int32 region_a_left, Int32 region_a_top,
            Int32 region_b_left, Int32 region_b_top,
            Int32 region_size)
        {
            Int32 x, y;
            byte val, region_a_darkest = byte.MaxValue, region_a_lightest = 0,
                region_b_darkest = byte.MaxValue, region_b_lightest = 0;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                {
                    val = (byte)a[region_a_left + x, region_a_top + y].shade;
                    if (val < region_a_darkest)
                        region_a_darkest = val;
                    if (val > region_a_lightest)
                        region_a_lightest = val;
                    val = (byte)b[region_b_left + x, region_b_top + y].shade;
                    if (val < region_b_darkest)
                        region_b_darkest = val;
                    if (val > region_b_lightest)
                        region_b_lightest = val;
                }
            byte region_a_darkest_from_lightest = (byte)(region_a_lightest - region_a_darkest),
                region_b_darkest_from_lightest = (byte)(region_b_lightest - region_b_darkest);
            float deviation = 0f;
            for (x = 0; x < region_size; x++)
                for (y = 0; y < region_size; y++)
                    deviation += Math.Abs(
                        ((a[region_a_left + x, region_a_top + y].shade - region_a_darkest)
                            / region_a_darkest_from_lightest) -
                        ((b[region_b_left + x, region_b_top + y].shade - region_b_darkest)
                            / region_b_darkest_from_lightest));
            return deviation;
        }

        private static img expand(img n, float scale)
        {
            Int32 x, y, n_width = n.RGB.GetLength(0), n_height = n.RGB.GetLength(1),
                result_width = (Int32)(n_width * scale), result_height = (Int32)(n_height * scale);
            var result = new img(result_width, result_height);
            float scale2 = 1f / scale;
            for (x = 0; x < result_width; x++)
                for (y = 0; y < result_height; y++)
                    result[x, y] = n[(Int32)(x * scale2), (Int32)(y * scale2)];
            return result;
        }

        private static img crop(img n, UInt16 x, UInt16 y, UInt16 size)
        {
            Int32 x2, y2, x3 = x, y3 = y;
            var result = new img(size, size);
            for (x2 = 0; x2 < size; x2++)
                for (y2 = 0; y2 < size; y2++)
                    result[x2, y2] = n[x3 + x2, y3 + y2];
            return result;
        }

        private class img
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

        private struct RGB
        {
            public byte r, g, b;

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

            public byte chan(byte c)
            {
                if (c == 0)
                    return r;
                else if (c == 1)
                    return g;
                else if (c == 2)
                    return b;
                return 0;
            }
        }

        private static void write(string s)
        {
            write2(s);
            indent++;
        }

        private static void write2(string s)
        {
            var s2 = "";
            for (byte n = 0; n < indent; n++)
                s2 += " ";
            Console.WriteLine(s2 + s);
        }

        private static void write3()
        {
            indent--;
        }
    }
}