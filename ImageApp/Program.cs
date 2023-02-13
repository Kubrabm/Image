using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace ImageApp
{
    internal partial class Program
    {
        static void Main(string[] args)
        {
            #region GetPixel
            //Pixels
            ComputeColors();

            Bitmap image = new Bitmap("C:\\Users\\User\\Desktop\\ImageApp\\ImageApp\\bin\\Debug\\net6.0\\pixel.png", true);
            DrawImage(image);
            #endregion

            #region GetImage
            //Point location = new Point(1, 1);
            //Size imageSize = new Size(50, 20); // desired image size in characters

            //// draw some placeholders
            //Console.SetCursorPosition(location.X - 1, location.Y);
            //Console.Write(">");
            //Console.SetCursorPosition(location.X + imageSize.Width, location.Y);
            //Console.Write("<");
            //Console.SetCursorPosition(location.X - 1, location.Y + imageSize.Height - 1);
            //Console.Write(">");
            //Console.SetCursorPosition(location.X + imageSize.Width, location.Y + imageSize.Height - 1);
            //Console.WriteLine("<");

            //string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonPictures), @"C:\Users\User\Desktop\ImageApp\ImageApp\bin\Debug\net6.0\baki.jpg");

            //using (Graphics g = Graphics.FromHwnd(GetConsoleWindow()))
            //{
            //    using (Image image = Image.FromFile(path))
            //    {
            //        Size fontSize = GetConsoleFontSize();

            //        // translating the character positions to pixels
            //        Rectangle imageRect = new Rectangle(
            //            location.X * fontSize.Width,
            //            location.Y * fontSize.Height,
            //            imageSize.Width * fontSize.Width,
            //            imageSize.Height * fontSize.Height);
            //        g.DrawImage(image, imageRect);
            //    }
            //}
            #endregion

        }

        #region ImageCont
        public static Size GetConsoleFontSize()
        {
            IntPtr outHandle = CreateFile("CONOUT$", GENERIC_READ | GENERIC_WRITE,
        FILE_SHARE_READ | FILE_SHARE_WRITE,
        IntPtr.Zero,
        OPEN_EXISTING,
        0,
        IntPtr.Zero);
            int errorCode = Marshal.GetLastWin32Error();
            if (outHandle.ToInt32() == INVALID_HANDLE_VALUE)
            {
                throw new IOException("Unable to open CONOUT$", errorCode);
            }

            ConsoleFontInfo cfi = new ConsoleFontInfo();
            if (!GetCurrentConsoleFont(outHandle, false, cfi))
            {
                throw new InvalidOperationException("Unable to get font information.");
            }

            return new Size(cfi.dwFontSize.X, cfi.dwFontSize.Y);
        }

        [DllImport("kernel32.dll", EntryPoint = "GetConsoleWindow", SetLastError = true)]
        private static extern IntPtr GetConsoleHandle();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFile(
            string lpFileName,
            int dwDesiredAccess,
            int dwShareMode,
            IntPtr lpSecurityAttributes,
            int dwCreationDisposition,
            int dwFlagsAndAttributes,
            IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetCurrentConsoleFont(
            IntPtr hConsoleOutput,
            bool bMaximumWindow,
            [Out][MarshalAs(UnmanagedType.LPStruct)] ConsoleFontInfo lpConsoleCurrentFont);

        [StructLayout(LayoutKind.Sequential)]
        internal class ConsoleFontInfo
        {
            internal int nFont;
            internal Coord dwFontSize;
        }

        [StructLayout(LayoutKind.Explicit)]
        internal struct Coord
        {
            [FieldOffset(0)]
            internal short X;
            [FieldOffset(2)]
            internal short Y;
        }

        private const int GENERIC_READ = unchecked((int)0x80000000);
        private const int GENERIC_WRITE = 0x40000000;
        private const int FILE_SHARE_READ = 1;
        private const int FILE_SHARE_WRITE = 2;
        private const int INVALID_HANDLE_VALUE = -1;
        private const int OPEN_EXISTING = 3;

        public static IntPtr ConsoleWindow { get; private set; }
        #endregion

        #region Pixel
        static List<ConsolePixel> pixels;
        private static void ComputeColors()
        {
            pixels = new List<ConsolePixel>();

            char[] chars = { '█', '▓', '▒', '░' };

            int[] rs = { 0, 0, 0, 0, 128, 128, 128, 192, 128, 0, 0, 0, 255, 255, 255, 255 };
            int[] gs = { 0, 0, 128, 128, 0, 0, 128, 192, 128, 0, 255, 255, 0, 0, 255, 255 };
            int[] bs = { 0, 128, 0, 128, 0, 128, 0, 192, 128, 255, 0, 255, 0, 255, 0, 255 };

            for (int i = 0; i < 16; i++)
                for (int j = i + 1; j < 16; j++)
                {
                    var l1 = RGBtoLab(rs[i], gs[i], bs[i]);
                    var l2 = RGBtoLab(rs[j], gs[j], bs[j]);

                    for (int k = 0; k < 4; k++)
                    {
                        var l = CieLab.Combine(l1, l2, (4 - k) / 4.0);

                        pixels.Add(new ConsolePixel
                        {
                            Char = chars[k],
                            Forecolor = (ConsoleColor)i,
                            Backcolor = (ConsoleColor)j,
                            Lab = l
                        });
                    }
                }
        }

        public static void DrawImage(Bitmap source)
        {
            int width = Console.WindowWidth - 1;
            int height = (int)(width * source.Height / 2.0 / source.Width);

            using (var bmp = new Bitmap(source, width, height))
            {
                var unit = GraphicsUnit.Pixel;
                using (var src = bmp.Clone(bmp.GetBounds(ref unit), PixelFormat.Format24bppRgb))
                {
                    var bits = src.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, src.PixelFormat);
                    byte[] data = new byte[bits.Stride * bits.Height];

                    Marshal.Copy(bits.Scan0, data, 0, data.Length);

                    for (int j = 0; j < height; j++)
                    {
                        StringBuilder builder = new StringBuilder();
                        var fore = ConsoleColor.White;
                        var back = ConsoleColor.Black;

                        for (int i = 0; i < width; i++)
                        {
                            int idx = j * bits.Stride + i * 3;
                            var pixel = DrawPixel(data[idx + 2], data[idx + 1], data[idx + 0]);


                            if (pixel.Forecolor != fore || pixel.Backcolor != back)
                            {
                                Console.ForegroundColor = fore;
                                Console.BackgroundColor = back;
                                Console.Write(builder);

                                builder.Clear();
                            }

                            fore = pixel.Forecolor;
                            back = pixel.Backcolor;
                            builder.Append(pixel.Char);
                        }

                        Console.ForegroundColor = fore;
                        Console.BackgroundColor = back;
                        Console.WriteLine(builder);
                    }

                    Console.ResetColor();
                }
            }
        }

        private static ConsolePixel DrawPixel(int r, int g, int b)
        {
            var l = RGBtoLab(r, g, b);

            double diff = double.MaxValue;
            var pixel = pixels[0];

            foreach (var item in pixels)
            {
                var delta = CieLab.DeltaE(l, item.Lab);
                if (delta < diff)
                {
                    diff = delta;
                    pixel = item;
                }
            }

            return pixel;
        }

        public static void ConsoleWriteImage(Bitmap bmpSrc)
        {
            int sMax = 39;
            decimal percent = Math.Min(decimal.Divide(sMax, bmpSrc.Width), decimal.Divide(sMax, bmpSrc.Height));
            Size resSize = new Size((int)(bmpSrc.Width * percent), (int)(bmpSrc.Height * percent));
            Func<System.Drawing.Color, int> ToConsoleColor = c =>
            {
                int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
                index |= (c.R > 64) ? 4 : 0;
                index |= (c.G > 64) ? 2 : 0;
                index |= (c.B > 64) ? 1 : 0;
                return index;
            };
            Bitmap bmpMin = new Bitmap(bmpSrc, resSize.Width, resSize.Height);
            Bitmap bmpMax = new Bitmap(bmpSrc, resSize.Width * 2, resSize.Height * 2);
            for (int i = 0; i < resSize.Height; i++)
            {
                for (int j = 0; j < resSize.Width; j++)
                {
                    Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMin.GetPixel(j, i));
                    Console.Write("██");
                }

                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("    ");

                for (int j = 0; j < resSize.Width; j++)
                {
                    Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2, i * 2));
                    Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2, i * 2 + 1));
                    Console.Write("▀");

                    Console.ForegroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2));
                    Console.BackgroundColor = (ConsoleColor)ToConsoleColor(bmpMax.GetPixel(j * 2 + 1, i * 2 + 1));
                    Console.Write("▀");
                }
                System.Console.WriteLine();
            }
        }

        public static int ToConsoleColor(System.Drawing.Color c)
        {
            int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0;
            index |= (c.R > 64) ? 4 : 0;
            index |= (c.G > 64) ? 2 : 0;
            index |= (c.B > 64) ? 1 : 0;
            return index;
        }
        #endregion
    }
}

