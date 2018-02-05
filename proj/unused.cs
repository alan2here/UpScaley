        /*private static img crop(img n, UInt16 x, UInt16 y, UInt16 size)
        {
            Int32 x2, y2, x3 = x, y3 = y;
            var result = new img(size, size);
            for (x2 = 0; x2 < size; x2++)
                for (y2 = 0; y2 < size; y2++)
                    result[x2, y2] = n[x3 + x2, y3 + y2];
            return result;
        }*/

        // crop "orig"
        // repeat "passes" times
        //   resize (width & height) of "orig" by "scale" times.
        //   Resolve "result" from ("orig" & "style") also using (the "score" function, "replacement size" & "skip").
        //   replace "style" with "result"
        // yeild result
        /*private static img resolve2(
            // params for "resolve"
            img orig, img style, UInt16 replacement_size,
            Func<img, img, Int32, Int32, Int32, Int32, Int32, float> score,
            byte verbosity = 3,

            // preparing "orig"
            UInt16? crop_x = null, UInt16? crop_y = null, UInt16? crop_size = null,

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
                    orig = expand.bicubic(orig, (float)scale);
                }
                result = style_transfer(orig, style, replacement_size, score, verbosity);
                style = result;
            }
            if (verbosity > 0)
                write3();
            return result;
        }*/