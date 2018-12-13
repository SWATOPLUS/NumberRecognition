using ImageRecognition;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace NumberRecognition
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private string[] paths;
        private PatternInfo[] patterns;
        private int id = 0;

        private void MainForm_Load(object sender, EventArgs e)
        {
            paths = Directory.GetFiles("cars/");
            patterns = Toolkit.LoadPatterns("patterns.json");
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            id--;
            if (id < 0)
            {
                id = paths.Length-1;
            }
            ApplyImage(paths[id]);
        }

        private void ApplyImage(string path)
        {
            Bitmap bitmap;
            using (var stream = Toolkit.Resize(File.ReadAllBytes(path), 640, 480))
            {
                bitmap = new Bitmap(stream);
            }
            SourceImage.Image = bitmap;

            var pic = new Picture(bitmap);

            double func(Picture picture) => (ImageManipulate.CalcNeighborPixel(picture, ImageManipulate.Contrast));

            var segmentSize = 8;
            var segY = 6;
            var segX = segY * 3;
            var map = pic.ApplySegmentFunc(segmentSize, func);
            var (x, y) = ImageManipulate.GetRect(map, segX, segY);
            map[x, y] = map[x + segX, y] = map[x, y + segY] = map[x + segX, y + segY] = -1;
            var mapBitmap = map.ToBitmap(segmentSize);
            ProcessedImage.Image = mapBitmap;

            var plateBitmap = bitmap.Clone(new Rectangle(x * segmentSize, y * segmentSize, segX * segmentSize, segY * segmentSize), mapBitmap.PixelFormat);
            PlateImage.Image = plateBitmap;
            var binBitmap = ImageManipulate.Binarize(plateBitmap);
            BinarizedImage.Image = binBitmap;

            CuttedImage.Image = ImageManipulate.CutOffMiddle(new Picture(binBitmap), 0.3).ToBitmap();

            var normalizeAngle = ImageManipulate.NormalizeAngle(binBitmap);
            var normalizedBitmap = binBitmap.RotateImage(normalizeAngle * 180.0 / Math.PI);
            NormalizedImage.Image = normalizedBitmap;
            //var fixedPic = ImageManipulate.FixBorder(new Picture(normalizedBitmap));
            //FixedImage.Image = fixedPic.ToBitmap();
            var clearedPic = ImageManipulate.ClearBorder(new Picture(normalizedBitmap));
            ClearedImage.Image = clearedPic.ToBitmap();

            var lets = ImageManipulate.GetLetters(clearedPic);

            if (lets.Length > 0)
            {
                pictureBox1.Image = lets[0].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 1)
            {
                pictureBox2.Image = lets[1].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 2)
            {
                pictureBox3.Image = lets[2].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 3)
            {
                pictureBox4.Image = lets[3].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 4)
            {
                pictureBox5.Image = lets[4].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 5)
            {
                pictureBox6.Image = lets[5].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 6)
            {
                pictureBox7.Image = lets[6].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 7)
            {
                pictureBox8.Image = lets[7].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 8)
            {
                pictureBox9.Image = lets[8].ToBitmap().ToNeiroArr().ToBitmap();
            }

            if (lets.Length > 9)
            {
                pictureBox10.Image = lets[9].ToBitmap().ToNeiroArr().ToBitmap();
            }

           

            var nums = lets
                .Where(g => g.Height * g.Width > 30)
                .Select(g => Toolkit.Match(g.ToBitmap().ToNeiroArr(), patterns))
                .Where(g => !g.Contains("EPS"))
                .Where(g => !g.Contains("MINUS"))
                .Where(g => !g.Contains("BRICK"))
                .ToArray();

            NumLabel.Text = string.Join("", nums);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            id++;
            if (id > paths.Length - 1)
            {
                id = 0;
            }
            ApplyImage(paths[id]);
        }
    }
}
