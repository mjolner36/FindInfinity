using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FindInfinity
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> inputImage = null;
        private bool isDrawing; 
        private Point previousPoint; 
        private Pen customPen = new Pen(Color.Red, 1);
        public Form1()
        {
            InitializeComponent();
            InitDrawPanel();
        }

        private void InitDrawPanel()
        {
            if (pictureBox1.Image == null)
            {
                // Создает пустое изображение с определенным размером и цветом фона
                Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White); // Заполняем фон белым цветом
                }
                pictureBox1.Image = bmp;
            }
            pictureBox1.MouseDown += PictureBox_MouseDown;
            pictureBox1.MouseMove += PictureBox_MouseMove;
            pictureBox1.MouseUp += PictureBox_MouseUp;
        }

        /// <summary>
        /// Начало рисования
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDrawing = true;
                previousPoint = e.Location;
            }
        }
        /// <summary>
        /// Рисование во время движения мышкой
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDrawing)
            {
                using (Graphics g = Graphics.FromImage(pictureBox1.Image))
                {                  
                    g.DrawLine(customPen, previousPoint, e.Location);
                }
                pictureBox1.Invalidate();
                previousPoint = e.Location;
            }
        }
        /// <summary>
        /// Закончить рисование
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDrawing = false;
            inputImage = new Image<Bgr, byte>((Bitmap)pictureBox1.Image);
        }

        /// <summary>
        /// Отчистить drawing panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.White); // Заполняем фон белым цветом
            }
            pictureBox1.Image = bmp;
        }

        /// <summary>
        /// Находим контуры изображения, а затем окружности
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            inputImage = new Image<Bgr, byte>((Bitmap)pictureBox1.Image);
            
            Image<Gray, byte> grayImage = inputImage.Convert<Gray, byte>();
            Image<Gray, byte> cannyEdges = grayImage.Canny(100, 255);

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();

            Mat hierarchy = new Mat();
            CvInvoke.FindContours(cannyEdges, contours, hierarchy, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            for (int i = 1; i < contours.Size; i++)
            {
                double perimeter = CvInvoke.ArcLength(contours[i], true);
                VectorOfPoint appoximation = new VectorOfPoint();

                CvInvoke.ApproxPolyDP(contours[i], appoximation, 0.04 * perimeter, true);
            }
            //находим окружности по контуру
            CircleF[] circles = CvInvoke.HoughCircles(cannyEdges, Emgu.CV.CvEnum.HoughType.Gradient, 3, inputImage.Rows / 9, 80, 80, 0, 80);

            if (IsLyingEight(circles))
            {
                MessageBox.Show("Это перевернутая восьмерка", "Ответ", MessageBoxButtons.OK);
            }
            else
            {
                MessageBox.Show("Это не перевернутая восьмерка", "Ответ", MessageBoxButtons.OK);
            }
        }

        /// <summary>
        /// Определяем, является ли две окружности перевернутой восьмеркой
        /// </summary>
        /// <param name="circleList"></param>
        /// <returns></returns>
        private bool IsLyingEight(CircleF[] circleList)
        {
            const double distanceThreshold = 130; // Максимальное расстояние между центрами кругов
            const double verticalAlignmentThreshold = 50; // Максимальное расхождение по вертикали
            const double radiusThreshold = 50;

            for (int i = 0; i < circleList.Length; i++)
            {
                for (int j = i + 1; j < circleList.Length; j++)
                {
                    CircleF circle1 = circleList[i];
                    CircleF circle2 = circleList[j];

                    double distance = Math.Sqrt(Math.Pow(circle1.Center.X - circle2.Center.X, 2) + Math.Pow(circle1.Center.Y - circle2.Center.Y, 2));
                    double radius1 = circle1.Radius;
                    double radius2 = circle1.Radius;
                   
                    double verticalDifference = Math.Abs(circle1.Center.Y - circle2.Center.Y);
                    // проверяем окружности на наличие лежачих восьмерок
                    // дистанция между окружностями, расхождение по вертикали, окружности пересекаются не более чем четверть от окружности 
                    if (distance <= distanceThreshold && verticalDifference <= verticalAlignmentThreshold && distance + radius1/4 >= radius1 + radius1)
                    {
                        return true;
                    }
                }
            }
            return false;
        }    
    }
}
