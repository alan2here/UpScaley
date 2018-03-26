// Code and concept by Alan Tennant, alan2here@gmail.com
// Some code and concept by Tim Wesson, tim.wesson@gmail.com, see comments.
// See https://github.com/alan2here/UpScaley
// Subject to a licence, see the link above.

using System;

namespace UpScaley
{
    public static class tables
    {
        public static float[,,] sqrt_sum = new float[256, 256, 256];
        public static byte[,] abs_dif = new byte[256, 256];
        public static Int16[,] abs_dif_retInt16 = new Int16[256, 256];
        public static float[,,] sqrt_sum_sqrs = new float[256, 256, 256];
        public static float[] recip_upTo769 = new float[770];
        public static float[] recip_upTo769_offset1 = new float[770];
        public static float[] recip_upTo769_offset5 = new float[770];
        public static float[] recip_upTo769_offset20 = new float[770];
        public static float[] recip_upTo769_offset100 = new float[770];
    }
}