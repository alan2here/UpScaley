// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
    public static class bicubic2
    {
        // code by Tim

        public static img expand(img n, float scale)
        {
            Int32 x, y, x_pos, y_pos, n_width = n.RGB.GetLength(0), n_height = n.RGB.GetLength(1),
                result_width = (Int32)(n_width * scale), result_height = (Int32)(n_height * scale);

            var result = new img(result_width, result_height);
            var image  = new img(n_width + 2, n_height + 2);

            // arrays run from 0 to n-1, so if we want ends to match, we need to scale appropriately
            float scale_inv_width  = (n_width - 1f) / (result_width - 1f);
            float scale_inv_height = (n_height - 1f) / (result_height - 1f);

            // avoid sampling off edge of image
            float scale_inv = 0.999999f * ((scale_inv_width < scale_inv_height) ? scale_inv_width : scale_inv_height); 
            float x_sample, y_sample, x_frac, y_frac;

            for (x = 0; x < n_width; x++)
            {
                image[x + 1, 0] = n[x, 0];
                for (y = 0; y < n_height; y++)
                    image[x + 1, y + 1] = n[x, y];
                image[x + 1, n_height + 1] = n[x, n_height - 1];
            }

            for (y = 0; y < n_height + 2; y++)
            {
                image[0, y] = image[1, y];
                image[n_width + 1, y] = image[n_width, y];
            }

            for (x = 0; x < result_width; x++)
            {
                x_sample = (float)(x * scale_inv);
                x_pos    = (Int32)x_sample;
                x_frac   = x_sample - (float)x_pos;

                for (y = 0; y < result_height; y++)
                {
                    y_sample = (float)(y * scale_inv);
                    y_pos    = (Int32)y_sample;
                    y_frac   = y_sample - (float)y_pos;

                    result[x, y] = bicubic_RGB_interpolator(image, x_pos, y_pos, x_frac, y_frac);
                }
            }
            return result;
        }

        // code by Tim
        private static float cubic_interpolator(float x, float p0, float p1, float p2, float p3)
        {
            return (p1 + 0.5f * x * (p2 - p0 + x * (2f * p0 - 5f * p1 + 4f * p2 - p3 + x * (3f * (p1 - p2) + p3 - p0))));
        }

        // code by Tim
        private static float bicubic_interpolator(float[,] interpolator_patch, float x_frac, float y_frac)
        {
            byte i;
            var cubic_intermediate = new float[4];

            for (i = 0; i < 4; i++)
                cubic_intermediate[i] = cubic_interpolator (x_frac, interpolator_patch[0,i],
                    interpolator_patch[1,i], interpolator_patch[2,i], interpolator_patch[3,i]);
            return cubic_interpolator (y_frac, cubic_intermediate[0], cubic_intermediate[1], cubic_intermediate[2], cubic_intermediate[3]);
        }

        // code by Tim
        private static RGB bicubic_RGB_interpolator(img image, Int32 x_pos, Int32 y_pos, float x_frac, float y_frac)
        {
            byte i, j;
            Int16 interpolated_value;
            var interpolator_patch = new float[4, 4];
            RGB return_value = new RGB();

            for (byte c = 0; c < 3; c++)
            {
                for (i = 0; i < 4; i++)
                    for (j = 0; j < 4; j++)
                        interpolator_patch[i, j] = image[x_pos + i, y_pos + j][c];

                // Clamping to prevent bad pixels as cubic interpolation can yield values out of range
                interpolated_value = (Int16)bicubic_interpolator(interpolator_patch, x_frac, y_frac);
                if (0 == (0xff00 & interpolated_value)) return_value[c] = (byte)interpolated_value;
                else  if (0 > interpolated_value)       return_value[c] = 0;
                else                                    return_value[c] = 0xff;
            }

            return return_value;
        }
    }
}