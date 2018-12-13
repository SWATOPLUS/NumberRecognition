using System;
using System.IO;
using System.Linq;
using ImageSharp;
using Newtonsoft.Json;

namespace ImageRecognition
{
    public static class Toolkit
    {
        public static Stream Resize(byte[] inStream, int? width = null, int? height = null)
        {
            var outStream = new MemoryStream();
            using (var stream = new MemoryStream(inStream))
            {
                var img = new ImageSharp.Image(stream);

                var w = width ?? img.Width;
                var h = height ?? img.Height;

                var result = img.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Pad,
                    Size = new Size(w, h)                    
                });


                result.SaveAsPng(outStream);

                return outStream;
            }
        }

        public static void SavePatterns(PatternInfo[] infos, string filename)
        {
            var json = JsonConvert.SerializeObject(infos);

            File.WriteAllText(filename, json);
        }

        public static PatternInfo[] LoadPatterns(string filename)
        {
            return JsonConvert.DeserializeObject<PatternInfo[]>(File.ReadAllText(filename));
        }

        public static string Match(int[,] g, PatternInfo[] patterns)
        {
            var min = double.MaxValue;
            var minName = "";

            foreach (var p in patterns)
            {
                var dist = GetDistance(g, p.Pattern);

                if (dist < min)
                {
                    min = dist;
                    minName = p.Name;
                }
            }

            return minName;
        }

        private static double GetDistance(int[,] arr, double[,] pattern)
        {
            var sum = 0.0;

            foreach (var x in Enumerable.Range(0,arr.GetLength(0)))
            {
                foreach (var y in Enumerable.Range(0, arr.GetLength(1)))
                {
                    var len = arr[x, y] - pattern[x, y];

                    sum += len * len;
                }
            }

            return sum;
        }
    }
}
