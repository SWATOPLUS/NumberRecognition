using System;
using Xunit;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using ImageRecognition;

namespace ToolsTests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var rnd = new Random();
            var files = Directory.GetFiles("d:/dataset/chars").OrderBy(x => rnd.Next()).ToArray();
            var dict = new Dictionary<string, List<int[,]>>();
            foreach (var x in files)
            {
                var name = Path.GetFileNameWithoutExtension(x);
                var ch = name.Split('_')[0];
                var arr = NeiroNet1.NeiroGraphUtils.LoadArrFromBitmap(x);
                if (!dict.ContainsKey(ch))
                {
                    dict.Add(ch,new List<int[,]>());
                }

                dict[ch].Add(arr);
            }

            var res = dict.Select(x => new PatternInfo
            {
                Name = x.Key,
                Pattern = Avg(x.Value)
            }).ToArray();

            Toolkit.SavePatterns(res, "patterns.json");
        }

        private double[,] Avg(IEnumerable<int[,]> elems)
        {
            var arrs = elems.ToArray();
            var xSize = arrs[0].GetLength(0);
            var ySize = arrs[0].GetLength(1);

            var res = new double[xSize, ySize];

            foreach (var x in Enumerable.Range(0, xSize))
            {
                foreach (var y in Enumerable.Range(0, ySize))
                {
                    res[x, y] = arrs.Select(e => e[x, y]).Average(); 
                }
            }

            return res;
        }
    }
}