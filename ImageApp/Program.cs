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
            Console.Write("Choose image path: ");

            ////dinamic
            string path = Console.ReadLine();

            ////static
            //string path1 = @"C:\Users\User\Desktop\ImageApp\ImageApp\bin\Debug\net6.0\pixel.png";

            Bitmap image = new Bitmap(path);

            for (int i = 0; i < image.Width; i++)
            {
                for (int j = 0; j < image.Height; j++)
                {
                    Color pixelColor = image.GetPixel(i, j);
                    int a = pixelColor.A;
                    int b = pixelColor.B;
                    int c = pixelColor.R;
                    int d = pixelColor.G;

                    int avg = (b + c + d) / 3;
                    
                    Color newColor = Color.FromArgb(a,avg,avg,avg);

                    image.SetPixel(i,j,newColor);
                }
            }

            Console.Write("Choose new image path: ");
            ////static
            //string newPath1 = @"C:\Users\User\Desktop\ImageApp\ImageApp\bin\Debug\net6.0\pixel1.png";

            ////dinamic
            string newPath = Console.ReadLine();
            image.Save(newPath);

            Console.WriteLine("DONE...");

        }
    }
    
}

