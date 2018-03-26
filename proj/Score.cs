// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
	public static class score
	{
        // sorry that this is so non-standard

        // index 0 is the default function and subject to change
        // index positions 1 and upwards are immutable

        // names of the non-deprecated functions, starting from 1:
            // col sim inc 1
            // col sim_inc 5
            // col sim inc 20
            // col sim
            // col dist
            // col
        
        // from 9:
            // col sim inc100

        // type type type type, typity type, typity type
        // repetition makes it safer, apparently
        // C# system/VS/compiler is being annoying
        public static Func<img, img, Int32, Int32, Int32, Int32, Int32, float> get(UInt16 n)
        {
            var r = new Func<img, img, Int32, Int32, Int32, Int32, Int32, float>(
                (img a, img b, Int32 c, Int32 d, Int32 e, Int32 f, Int32 g) => (0f)
            );

            switch (n)
            {
                case 0: // default, currently 2 -> "col sim inc 5"
                    r = get(2); break;
                case 1: // col sim inc 1
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation -= tables.recip_upTo769_offset1[
                                    tables.abs_dif_retInt16[a2.r, b2.r] +
                                    tables.abs_dif_retInt16[a2.g, b2.g] +
                                    tables.abs_dif_retInt16[a2.b, b2.b]];
                            }
                        return deviation;
                    };
                    break;
                case 2: // col sim inc 5
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        // GPU proposal 'd', see style_transfer in UpScaley.cs
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation -= tables.recip_upTo769_offset5[
                                    tables.abs_dif_retInt16[a2.r, b2.r] +
                                    tables.abs_dif_retInt16[a2.g, b2.g] +
                                    tables.abs_dif_retInt16[a2.b, b2.b]];
                            }
                        return deviation;
                        // end 'd'
                    };
                    break;
                case 3: // col sim inc20
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation -= tables.recip_upTo769_offset20[
                                    tables.abs_dif_retInt16[a2.r, b2.r] +
                                    tables.abs_dif_retInt16[a2.g, b2.g] +
                                    tables.abs_dif_retInt16[a2.b, b2.b]];
                            }
                        return deviation;
                    };
                    break;
                case 4: // col sim
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation -= tables.recip_upTo769[
                                    tables.abs_dif_retInt16[a2.r, b2.r] +
                                    tables.abs_dif_retInt16[a2.g, b2.g] +
                                    tables.abs_dif_retInt16[a2.b, b2.b]];
                            }
                        return deviation;
                    };
                    break;
                case 5: // col dist
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation += tables.sqrt_sum_sqrs[
                                    tables.abs_dif[a2.r, b2.r], tables.abs_dif[a2.g, b2.g], tables.abs_dif[a2.b, b2.b]];
                            }
                        return deviation;
                    };
                    break;
                case 6: // col
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation += tables.abs_dif[a2.r, b2.r] + tables.abs_dif[a2.g, b2.g] + tables.abs_dif[a2.b, b2.b];
                            }
                        return deviation;
                    };
                    break;
                case 7: // grey
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                                deviation += (byte)Math.Abs(a[region_a_left + x, region_a_top + y].shade -
                                    b[region_b_left + x, region_b_top + y].shade);
                        return deviation;
                    };
                    break;
                case 8: // grey_nprmal
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
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
                    };
                    break;
                case 9: // col sim inc100
                    r = (img a, img b, Int32 region_a_left, Int32 region_a_top,
                        Int32 region_b_left, Int32 region_b_top, Int32 region_size) =>
                    {
                        Int32 x, y;
                        RGB a2, b2;
                        float deviation = 0f;
                        for (x = 0; x < region_size; x++)
                            for (y = 0; y < region_size; y++)
                            {
                                a2 = a[region_a_left + x, region_a_top + y];
                                b2 = b[region_b_left + x, region_b_top + y];
                                deviation -= tables.recip_upTo769_offset100[
                                    tables.abs_dif_retInt16[a2.r, b2.r] +
                                    tables.abs_dif_retInt16[a2.g, b2.g] +
                                    tables.abs_dif_retInt16[a2.b, b2.b]];
                            }
                        return deviation;
                    };
                    break;
                default:
                    throw new ArgumentException("No such scoring function, try 0 or 1 to 8.");
            }
            return r;
        }
    }
}