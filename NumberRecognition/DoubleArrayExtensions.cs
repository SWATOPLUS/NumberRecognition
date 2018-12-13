using NeiroNet1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace NumberRecognition
{
    public static class DoubleArrayExtensions
    {
        public static Bitmap ToBitmap(this double[,] arr, int segmentSize)
        {
            var width = arr.GetLength(0);
            var height = arr.GetLength(1);

            var bitmap = new Bitmap(width * segmentSize, height * segmentSize);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var color = arr[i, j].ToColor();

                    for (int x = 0; x < segmentSize; x++)
                    {
                        for (int y = 0; y < segmentSize; y++)
                        {
                            bitmap.SetPixel(i * segmentSize + x, j * segmentSize + y, color);
                        }
                    }
                }
            }

            return bitmap;
        }

        private static Color ToColor(this double value)
        {
            var brightness = (int)Math.Round(value * 255);

            if (brightness < 0)
            {
                return Color.Red;
            }

            return Color.FromArgb(brightness, brightness, brightness);
        }

        public static int[,] ToNeiroArr(this Bitmap img)
        {
            //var clipArr = NeiroGraphUtils.CutImageToArray(img, new Point(img.Width, img.Height));
            var clipArr = new Picture(img).TrimUpper().ToBitmap().Invert();
            clipArr.Save("temp.img");
            var bytes = File.ReadAllBytes("temp.img");
            return new Bitmap(ImageRecognition.Toolkit.Resize(bytes, 30, 30)).ToArr();//NeiroGraphUtils.LeadArray(clipArr, new int[10, 10]);
        }

        private static int[,] ToArr(this Bitmap img)
        {
            var res = new int[img.Width, img.Height];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    res[i, j] = img.GetPixel(i, j).R == 0 ? 0 : 1;
                }
            }

            return res;
        }

        private static Bitmap Invert(this Bitmap img)
        {
            var res = new Bitmap(img.Width, img.Height);
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    var color = img.GetPixel(i, j).R == 0 ? Color.White : Color.Black;
                    res.SetPixel(i,j,color);
                }
            }

            return res;
        }

        public static Bitmap ToBitmap(this int[,] arr)
        {
            var Width = arr.GetLength(0);
            var Height = arr.GetLength(1);
            var bitmap = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    bitmap.SetPixel(i, j, arr[i,j] > 0 ? Color.White : Color.Black);
                }
            }

            return bitmap;
        }
    }
}
