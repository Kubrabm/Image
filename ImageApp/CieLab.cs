namespace ImageApp
{
    internal partial class Program
    {
        public class CieLab
        {
            public double L { get; set; }
            public double A { get; set; }
            public double B { get; set; }

            public static double DeltaE(CieLab l1, CieLab l2)
            {
                return Math.Pow(l1.L - l2.L, 2) + Math.Pow(l1.A - l2.A, 2) + Math.Pow(l1.B - l2.B, 2);
            }

            public static CieLab Combine(CieLab l1, CieLab l2, double amount)
            {
                var l = l1.L * amount + l2.L * (1 - amount);
                var a = l1.A * amount + l2.A * (1 - amount);
                var b = l1.B * amount + l2.B * (1 - amount);

                return new CieLab { L = l, A = a, B = b };
            }
        }

        public static CieLab RGBtoLab(int red, int green, int blue)
        {
            var rLinear = red / 255.0;
            var gLinear = green / 255.0;
            var bLinear = blue / 255.0;

            double r = rLinear > 0.04045 ? Math.Pow((rLinear + 0.055) / (1 + 0.055), 2.2) : (rLinear / 12.92);
            double g = gLinear > 0.04045 ? Math.Pow((gLinear + 0.055) / (1 + 0.055), 2.2) : (gLinear / 12.92);
            double b = bLinear > 0.04045 ? Math.Pow((bLinear + 0.055) / (1 + 0.055), 2.2) : (bLinear / 12.92);

            var x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            var y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            var z = r * 0.0193 + g * 0.1192 + b * 0.9505;

            Func<double, double> Fxyz = t => ((t > 0.008856) ? Math.Pow(t, (1.0 / 3.0)) : (7.787 * t + 16.0 / 116.0));

            return new CieLab
            {
                L = 116.0 * Fxyz(y / 1.0) - 16,
                A = 500.0 * (Fxyz(x / 0.9505) - Fxyz(y / 1.0)),
                B = 200.0 * (Fxyz(y / 1.0) - Fxyz(z / 1.0890))
            };
        }

    }
}
