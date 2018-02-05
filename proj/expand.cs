using System;

namespace UpScaley
{
    public static class expand
    {
        public static img nearest(img n, float scale)
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

        public static img bicubic(img n, float scale)
        {
            return bicubic2.expand(n, scale);
        }
    }
}