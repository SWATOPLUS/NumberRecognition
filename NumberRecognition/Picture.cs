using System;
using System.Drawing;

namespace NumberRecognition
{
    public class Picture
    {
        private int x, y;
        private Color[,] arr;

        public Color this[int i, int j] => arr[x + i, y + j];
        public int Height { get; }
        public int Width { get; }

        public Picture(Color[,] arr, int x, int y, int h, int w)
        {
            Height = h;
            Width = w;
            this.x = x;
            this.y = y;
            this.arr = arr;
        }

        public Picture(Picture pic, int x, int y, int h, int w) : this(pic.arr, x, y, h, w)
        {
        }        

        public Picture(Color[,] arr) : this(arr, 0, 0, arr.GetLength(1), arr.GetLength(0))
        {
        }

        public Picture(Bitmap src, int width, int height)
        {
            arr = new Color[width, height];
            x = y = 0;
            Height = height;
            Width = width;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    arr[i, j] = src.GetPixel(i, j);
                }
            }
        }

        public Picture(Bitmap src) : this(src, src.Width, src.Height)
        {
        }

        public Picture TrimUpper()
        {
            var hist = new int[Height];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if(arr[i,j].R == 0)
                        hist[j]++;
                }
            }

            int up = 0;
            int down = Height - 1;

            for (; up < down; up++)
            {
                if (hist[up] > 2)
                    break;
            }

            for (; up < down; down--)
            {
                if (hist[down] > 2)
                    break;
            }

            return new Picture(this, 0, up, down - up + 1, Width);
        }

        public Bitmap ToBitmap()
        {
            var bitmap = new Bitmap(Width, Height);

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    bitmap.SetPixel(i, j, arr[x + i, y + j]);
                }
            }

            return bitmap;
        }

        public double[,] ApplySegmentFunc(int segmentSize, Func<Picture, double> func)
        {
            var w = Width / segmentSize;
            var h = Height / segmentSize;

            var vals = new double[w, h];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var segment = new Picture(arr, i * segmentSize, j * segmentSize, segmentSize, segmentSize);
                    var val = func(segment);

                    vals[i, j] = val;
                }
            }

            return vals;
        }
    }
}
