namespace UpScaley
{
    public class writer
    {
        private static byte indent = 0;

        public static void write_then_inc(string s)
        {
            write(s);
            indent++;
        }

        public static void write(string s)
        {
            var s2 = "";
            for (byte n = 0; n < indent; n++)
                s2 += " ";
            System.Console.WriteLine(s2 + s);
        }

        public static void dec()
        {
            indent--;
        }
    }
}