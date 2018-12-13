using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NumberRecognition
{
    public static class ImageManipulate
    {
        public static (int, int) GetRect(double[,] map, int segmentsX, int segmentsY)
        {
            var w = map.GetLength(0) - segmentsX;
            var h = map.GetLength(1) - segmentsY;

            var maxSum = 0.0;
            var maxX = 0;
            var maxY = 0;

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    var sum = 0.0;

                    for (int x = 0; x < segmentsX; x++)
                    {
                        for (int y = 0; y < segmentsY; y++)
                        {
                            sum += map[i + x, j + y];
                        }
                    }

                    if (sum > maxSum)
                    {
                        maxSum = sum;
                        maxX = i;
                        maxY = j;
                    }
                }
            }

            return (maxX, maxY);
        }

        public static double CalcSingePixel(Picture segment, Func<Color, double> func)
        {
            var w = segment.Width;
            var h = segment.Height;

            var vals = new List<double>();

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    vals.Add(func(segment[i, j]));
                }
            }

            return vals.Average();
        }

        public static double CalcNeighborPixel(Picture segment, Func<Color[,], double> func)
        {
            var w = segment.Width;
            var h = segment.Height;

            if (w < 3 || h < 3)
            {
                throw new InvalidOperationException();
            }

            var vals = new List<double>();

            for (int i = 1; i < w - 1; i++)
            {
                for (int j = 1; j < h - 1; j++)
                {
                    var arr = CopyNeighbors(segment, i, j);

                    vals.Add(func(arr));
                }
            }

            return vals.Average();
        }

        public static Color[,] CopyNeighbors(Picture segment, int x, int y)
        {
            var arr = new Color[3, 3];

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    arr[i, j] = segment[x - 1 + i, y - 1 + j];
                }
            }

            return arr;
        }

        public static double GreyDistance(Color color)
        {
            var avg = ((color.R + color.G + color.B)) / 3.0;

            return Math.Round((Math.Abs(avg - color.R) + Math.Abs(avg - color.G) + Math.Abs(avg - color.B)) / 339.0);
        }

        public static double ColorDistance(Color x, Color y)
        {
            return (Math.Abs(x.R - y.R) + Math.Abs(x.G - y.G) + Math.Abs(x.B - y.B)) / (255 * 3.0);
        }

        public static double Contrast(Color[,] colors)
        {
            var center = colors[1, 1];
            var distance = colors[1, 1];

            var vals = new List<double>();

            foreach (var x in colors)
            {
                vals.Add(ColorDistance(x, center));
            }

            return vals.Average();
        }

        public static Bitmap Binarize(Bitmap src, double level = 0.5)
        {
            var res = new Bitmap(src.Width, src.Height);

            for (var i = 0; i < src.Width; i++)
            {
                for (var j = 0; j < src.Height; j++)
                {
                    var color = src.GetPixel(i, j);
                    var light = (color.R + color.G + color.B) > (level * 3 * 255);
                    var rescolor = light ? Color.White : Color.Black;

                    res.SetPixel(i, j, rescolor);
                }
            }

            return res;
        }

        public static double NormalizeAngle(Bitmap src, double part = 0.3)
        {
            var pic = CutOffMiddle(new Picture(src), part);

            var max = 0.0;
            var maxtg = 0.0;

            foreach (var x in Enumerable.Range(-15, 15))
            {
                var tg = x * 0.01;

                var balance = CalcBalance(pic, tg);

                if (balance > max)
                {
                    max = balance;
                    maxtg = tg;
                }
            }

            return Math.Atan(-maxtg);
        }

        private static double CalcBalance(Picture pic, double tg)
        {
            var arr = Enumerable.Range(0, pic.Height).Select(x => CalcBalanceLine(pic, x, tg)).ToArray();

            return arr.Sum();
        }

        private static double CalcBalanceLine(Picture pic, int line, double tg)
        {
            var pos = 1;
            var b = 0.0;
            var w = 0.0;

            try
            {
                while (true)
                {
                    var x = pos;
                    var y = (int)Math.Round(line + x * tg);

                    if (pic[x, y].B == 0)
                    {
                        b++;
                    }
                    else
                    {
                        w++;
                    }
                    

                    pos++;
                }
            }
            catch (IndexOutOfRangeException)
            {
            }

            var sum = b + w;

            if (sum < 3) return 0;

            return Math.Max(b / sum, w / sum) * Math.Sqrt(sum);
        }

        public static Picture CutOffMiddle(Picture pic, double part)
        {
            var h = pic.Height;
            var w = pic.Width;
            var firstLine = (int)Math.Round(h * part);
            var secondLine = (int)Math.Round(h * (1 - part));
            var arr = new Color[w, firstLine + h - secondLine];

            for (var i = 0; i < w; i++)
            {
                for (var j = 0; j < firstLine; j++)
                {
                    arr[i, j] = pic[i, j];
                }

                for (var j = secondLine; j < h; j++)
                {
                    arr[i, j - secondLine + firstLine] = pic[i, j];
                }
            }

            return new Picture(arr);
        }

        public static Picture FixBorder(Picture pic, double part = 0.4, double cutpercent = 0.6)
        {
            var h = pic.Height;
            var w = pic.Width;

            var firstLine = (int)Math.Round(h * part);
            var secondLine = (int)Math.Round(h * (1 - part));
            var cut = w * cutpercent;

            var hist = new double[h];

            for (int i = 0; i < w; i++)
            {
                for(int j= 0;j< h; j++)
                {
                    hist[j] += pic[i, j].R == 0 ? 1 : 0; 
                }
            }

            for (; firstLine >= 0; firstLine--)
            {
                if (hist[firstLine] > cut)
                {
                    break;
                }
            }

            for (; secondLine < h; secondLine++)
            {
                if (hist[secondLine] > cut)
                {
                    break;
                }
            }

            return new Picture(pic, 0, firstLine, secondLine - firstLine, w);
        }

        public static Picture ClearBorder(Picture pic)
        {
            var list = new List<(int x, int y)>();
            var pos = 0;

            for (int i = 0; i < pic.Width; i++)
            {
                list.Add((i, 0));
                list.Add((i, pic.Height - 1));
            }

            for (int i = 0; i < pic.Height; i++)
            {
                list.Add((0, i));
                list.Add((pic.Width -1, 0));
            }

            while (pos != list.Count)
            {
                var (x,y) = list[pos];

                TryAdd(pic, list, x+1, y);
                TryAdd(pic, list, x-1, y);
                TryAdd(pic, list, x, y-1);
                TryAdd(pic, list, x, y+1);

                pos++;
            }

            var arr = new Color[pic.Width, pic.Height];

            for (int i = 0; i < pic.Width; i++)
            {
                for (int j = 0; j < pic.Height; j++)
                {
                    arr[i, j] = pic[i, j];
                }
            }

            foreach (var (x, y) in list)
            {
                arr[x, y] = Color.White;
            }

            return new Picture(arr);
        }

        private static void TryAdd(Picture pic, List<(int x, int y)> list, int x, int y)
        {
            if (x < 0 || y < 0 || x >= pic.Width || y >= pic.Height)
            {
                return;
            }

            if (pic[x, y].R != 0)
            {
                return;
            }

            if (list.Contains((x, y)))
            {
                return;
            }

            list.Add((x, y));
        }

        public static Picture[] GetLetters(Picture pic)
        {
            var hist = new int[pic.Width];

            for (int i = 0; i < pic.Width;i++)
            {
                for (int j = 0; j < pic.Height; j++)
                {
                    if (pic[i, j].R == 0)
                    {
                        hist[i]++;
                    }
                }
            }

            var list = new List<Picture>();
            var lastVert = 0;

            for (int i = 1; i < pic.Width; i++)
            {
                if (hist[i] * 25 < pic.Height)
                {
                    list.Add(new Picture(pic, lastVert, 0, pic.Height, i - lastVert));
                    lastVert = i;
                }                
            }

            return list.Where(x => x.Width > 3).ToArray();
        }

    }
}
