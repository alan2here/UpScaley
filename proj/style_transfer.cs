// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;
using System.Threading;

namespace UpScaley
{
    public static class style_transfer
    {
        // cannot process close to the edge yet
        public static img trans(img Base, img style,
            Func<img, img, Int32, Int32, Int32, Int32, Int32, float> score,
            UInt16 replacement_size, byte verbosity = 3)
        {
            if (verbosity > 4)
                throw new ArgumentException("4 is the maximum verbosity");
            if (score == null)
                throw new ArgumentException("A scoring function is required.");
            Int32 replacement_size2 = replacement_size;
            if (replacement_size2 % 2 == 0)
                throw new ArgumentException("replacement_size must be an odd number");
            UInt32 denominator = (UInt32)(verbosity > 3 ? 1000000 : 1000);
            byte numerator = 1, least_denominator = verbosity > 3 ? (byte)100 : (byte)20, cycle_pos = 0, best_deviations_count = 5;
            Int32 base_x, base_y,
                base_and_result_width = Base.RGB.GetLength(0), base_and_result_height = Base.RGB.GetLength(1),
                style_width = style.RGB.GetLength(0), style_height = style.RGB.GetLength(1),
                base_and_result_width_subtract_replacment_size = base_and_result_width - replacement_size2,
                base_and_result_height_subtract_replacment_size = base_and_result_height - replacement_size2,
                style_width_subtract_replacment_size = style_width - replacement_size2,
                style_height_subtract_replacment_size = style_height - replacement_size2,
                half_replacement_size = (Int32)(replacement_size2 * 0.5f);
            var result = new img(base_and_result_width, base_and_result_height);
            UInt64 pixel_count = 0, expected_pixels = (UInt64)(base_and_result_width_subtract_replacment_size *
                                                               base_and_result_height_subtract_replacment_size);

            if (verbosity > 0)
                writer.write_then_inc("style transfer");
            var region_comparisons = (UInt64)base_and_result_width_subtract_replacment_size *
                                     (UInt64)base_and_result_height_subtract_replacment_size *
                                     (UInt64)style_width_subtract_replacment_size *
                                     (UInt64)style_height_subtract_replacment_size;
            if (verbosity > 1)
            {
                writer.write("replacement_size: " + replacement_size);
                writer.write("The load is about " +
                             (UInt64)Math.Round(region_comparisons * (UInt64)replacement_size * (UInt64)replacement_size * 0.000000001f) +
                             " billion pixel pair comparisons.");
            }
            if (verbosity > 2)
            {
                writer.write("Or (" + (UInt64)Math.Round(region_comparisons * 0.000001f) +
                             " million region comparisons) or (" + (UInt64)Math.Round(expected_pixels * 0.001f) +
                             " thousand pixels of base/result resolved (" + expected_pixels + ")).");
                writer.write("base and result width: " + base_and_result_width);
                writer.write("base and result height: " + base_and_result_height);
                writer.write("style width: " + style_width);
                writer.write("style height: " + style_height);
                writer.write("base and result width subtract replacment size: " + base_and_result_width_subtract_replacment_size);
                writer.write("base and result height subtract replacment size: " + base_and_result_height_subtract_replacment_size);
                writer.write("style width subtract replacment size: " + style_width_subtract_replacment_size);
                writer.write("style height subtract replacment size: " + style_height_subtract_replacment_size);
            }

            for (base_x = 0; base_x < base_and_result_width_subtract_replacment_size; base_x++)
            {
                for (base_y = 0; base_y < base_and_result_height_subtract_replacment_size; base_y++)
                {
                    if (verbosity > 1)
                        while ((float)pixel_count / expected_pixels > (float)numerator / denominator)
                        {
                            writer.write("" + numerator + " / " + denominator);
                            if (denominator == least_denominator)
                                numerator++;
                            else
                                denominator = (UInt32)(denominator * (cycle_pos % 3 == 1 ? 0.4f : 0.5f));
                            cycle_pos++;
                        }
                    pixel_count++;

                    var best_deviations_rgb = new RGB[style_width_subtract_replacment_size, best_deviations_count];
                    var best_deviations = new float[style_width_subtract_replacment_size, best_deviations_count];
                    {
                        byte pos, pos2;
                        for (pos = 0; pos < style_width_subtract_replacment_size; pos++)
                        for (pos2 = 0; pos2 < best_deviations_count; pos2++)
                            best_deviations[pos, pos2] = float.MaxValue;
                    }

                    Int32 pass = 0, worst_x = 0;
                    var best_columns = new Int32[best_deviations_count];
                    var best_columns_value = new float[best_deviations_count];
                    var mut = new Mutex();

                    // OpenCL/CUDA (or GLSL even) here, proposal 'a'.
                    System.Threading.Tasks.Parallel.For(0, style_width_subtract_replacment_size, style_x =>
                    {
                        // GPU proposal 'b'.
                        byte pos;
                        float best_value = float.MaxValue;
                        Int32 worst_pos = 0;
                        for (Int32 style_y = 0; style_y < style_height_subtract_replacment_size; style_y++)
                        {
                            // GPU proposal 'c', see col_sim_inc5 in Score.cs for proposal 'd'.
                            float deviation = score(Base, style, base_x, base_y, style_x, style_y, replacement_size2);
                            // end 'c'
                            pass++;

                            if (deviation <= best_deviations[style_x, worst_pos])
                            {
                                best_deviations_rgb[style_x, worst_pos] = style[style_x + half_replacement_size, style_y + half_replacement_size];
                                best_deviations[style_x, worst_pos] = deviation;
                                for (pos = 0; pos < best_deviations_count; pos++)
                                    if (best_deviations[style_x, pos] > best_deviations[style_x, worst_pos])
                                        worst_pos = pos;

                                // Need the best value, as the best columns are those containing by the best matches.
                                if (deviation < best_value)
                                    best_value = deviation;
                            }
                        }
                        // end 'b'

                        // code by Tim
                        {
                            mut.WaitOne();

                            // The n best elements are contained in the n best columns, as ordered by best match in that column.
                            if (best_value < best_columns_value[worst_x])
                            {
                                // Replace the worst x with a better one (mutex avoids race condition).
                                best_columns[worst_x] = style_x;
                                best_columns_value[worst_x] = best_value;
                                // Find the new worst x.
                                for (pos = 0; pos < best_deviations_count; pos++)
                                    if (best_columns_value[pos] > best_columns_value[worst_x])
                                        worst_x = pos;
                            }

                            mut.ReleaseMutex();
                        }
                    });
                    // end 'a'

                    result[base_x + half_replacement_size, base_y + half_replacement_size] =
                        RGB_from_candidate_matches(best_deviations, best_deviations_rgb, best_columns);
                }
            }

            if (verbosity > 0)
            {
                writer.write("Pixels of base/result calculated: " + pixel_count);
                if (pixel_count < expected_pixels)
                    writer.write("ERROR: Skipped some pixels?");
                if (pixel_count > expected_pixels)
                    writer.write("ERROR: Duplicated pixels?");
                writer.dec();
            }

            return result;
        }

        private static RGB RGB_from_candidate_matches(float[,] best_deviations, RGB[,] best_deviations_rgb, Int32[] best_columns) // code by Tim
        {
            byte bubble_end, pos, worst_pos, x_val, y_val, best_deviations_count = (byte)best_deviations.GetLength(1);
            var best_deviations_selection = new Int32[best_deviations_count, 2];
            var best_deviations_selection_value = new float[best_deviations_count];
            var weighted_average = new float[]{0, 0, 0};
            float weighting, weighted_correction; // factor = 0.7f;

            // initialise worst_pos of best_deviations_selection to force sensible first comparison.
            worst_pos = 0;
            best_deviations_selection[0,0] = best_columns[0];
            best_deviations_selection[0,1] = 0;
            for (pos = 0; pos < best_deviations_count; pos++)
                best_deviations_selection_value[pos] = float.MaxValue;  // to be overwritten

            // From the n best rows, and the n best elements of those rows, we extract the n best results.
            for (x_val = 0; x_val < best_deviations_count; x_val++)
            for (y_val = 0; y_val < best_deviations_count; y_val++)
            {
                // Replace the worst value with the new one.
                best_deviations_selection[worst_pos, 0]    = best_columns[x_val];
                best_deviations_selection[worst_pos, 1]    = y_val;
                best_deviations_selection_value[worst_pos] = best_deviations[best_columns[x_val], y_val];

                // Find the new worst value.
                for (pos = 0; pos < best_deviations_count; pos++)
                    if (best_deviations_selection_value[pos] > best_deviations_selection_value[worst_pos])
                        worst_pos = pos;
            }

            for (bubble_end = (byte)(best_deviations_count - 1); bubble_end > 0; bubble_end--)
            {
                // Find the next worst match.
                for  (pos = 0, worst_pos = bubble_end; pos < bubble_end; pos++)
                    if (best_deviations_selection_value[pos] > best_deviations_selection_value[worst_pos])
                        worst_pos = pos;

                // weighted average, n-th place get a weight of 1/n.

                weighting = 1f / (bubble_end + 1f);
                // weighting = (float)Math.Pow((double)factor, (bubble_end + 1d));
                // Instead of storing the bubble order, we evaluate the weighted average on the go from the worst to the best.
                for (pos = 0; pos < 3; pos++) // Red, Green, and Blue.
                    weighted_average[pos] += weighting * (float)best_deviations_rgb[best_deviations_selection[0, 0], best_deviations_selection[0, 1]][pos];

                // Shuffle the better result back into our list, overwriting the old worst.
                best_deviations_selection[worst_pos, 0] = best_deviations_selection[bubble_end, 0];
                best_deviations_selection[worst_pos, 1] = best_deviations_selection[bubble_end, 1];
                best_deviations_selection_value[worst_pos] = best_deviations_selection_value[bubble_end];
            }

            for (pos = best_deviations_count, weighted_correction = 0; pos > 0; weighted_correction += 1f/pos, pos--) {}
            // weighted_correction = (1f - factor)/(1f - (float)Math.Pow((double)factor, best_deviations_count));

            for (pos = 0; pos < 3; pos++) // Red, Green, and Blue.  Weighting is 1f.
            {
                weighted_average[pos] += (float)best_deviations_rgb[best_deviations_selection[0, 0], best_deviations_selection[0, 1]][pos];
                weighted_average[pos] /= weighted_correction;
            }

            // +0.5f so as to implement rounding.
            return new RGB((byte)(weighted_average[0] + 0.5f), (byte)(weighted_average[1] + 0.5f), (byte)(weighted_average[2] + 0.5f));
        }
    }
}