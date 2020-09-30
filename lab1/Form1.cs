using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace lab1
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> sourceImage;
        private Image<Gray, byte> cannyEdges;
        double T = 80.0;
        double TL = 40.0;

        private VideoCapture capture;
        int frameCount = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {  
            loadImage();
            canniEf(sourceImage);
            canniEdg(sourceImage);
        }

        private void loadImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                sourceImage = new Image<Bgr, byte>(fileName);
            }
            if(sourceImage != null)
            imageBox1.Image = sourceImage.Resize(1280, 960, Inter.Linear);
        }


        private void canniEf(Image<Bgr, byte> sourceImage)
        {
            double cannyThreshold = T;
            double cannyThresholdLinking = TL;

            cannyEdges = sourceImage.Canny(cannyThreshold, cannyThresholdLinking);
            imageBox2.Image = cannyEdges.Resize(640, 480, Inter.Linear);
        }

        private void canniEdg(Image<Bgr, byte> sourceImage)
        {
            var cannyEdgesBgr = cannyEdges.Convert<Bgr, byte>();
            var resultImage = sourceImage.Sub(cannyEdgesBgr); // попиксельное вычитание
                                                              //обход по каналам
            for (int channel = 0; channel < resultImage.NumberOfChannels; channel++)
                for (int x = 0; x < resultImage.Width; x++)
                    for (int y = 0; y < resultImage.Height; y++) // обход по пискелям
                    {
                        // получение цвета пикселя
                        byte color = resultImage.Data[y, x, channel];
                        if (color <= 50)
                            color = 0;
                        else if (color <= 100)
                            color = 25;
                        else if (color <= 150)
                            color = 180;
                        else if (color <= 200)
                            color = 210;
                        else
                            color = 255;
                        resultImage.Data[y, x, channel] = color; // изменение цвета пикселя
                    }

            imageBox3.Image = resultImage.Resize(640, 480, Inter.Linear); 
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (textBox3.Text != String.Empty)
            {
                T = double.Parse(textBox3.Text);
            }
            else
            {
                T = 1;
            }
            
            if (T >= 0 && T <= 100) 
                tbChanged(T);
            else
            {
                MessageBox.Show("Выход за допустимые значения. Диапазон 0-100");
                textBox3.Clear();
            }
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (textBox4.Text != String.Empty)
            {
                T = double.Parse(textBox4.Text);
            }
            else
            {
                T = 1;
            }

            if (T >= 0 && T <= 100)
                tbChanged(T);
            else
            {
                MessageBox.Show("Выход за допустимые значения. Диапазон 0-100");
                textBox4.Clear();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = openFileDialog.FileName;
                capture = new VideoCapture(fileName);

                timer1.Enabled = true;
            }
            //imageBox1.Image = sourceImage.Resize(320, 240, Inter.Linear); 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();

            imageBox1.Image = frame;

            frameCount++;

            if(frameCount >= capture.GetCaptureProperty(CapProp.FrameCount))
            {
                timer1.Enabled = false;
            }
            
            sourceImage = frame.ToImage<Bgr, byte>();
            canniEf(sourceImage);
            canniEdg(sourceImage);

            frameCount = 0;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            capture = new VideoCapture();
            capture.ImageGrabbed += ProcessFrame;
            capture.Start(); // начало обработки видеопотока
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            var frame = new Mat();
            capture.Retrieve(frame); // получение текущего кадра

            sourceImage = frame.ToImage<Bgr, byte>();
            canniEf(sourceImage);
            canniEdg(sourceImage);

            //imageBox3.Image = image;
        }
        private void tbChanged(double tb)
        {
            T = tb;
            if (sourceImage != null)
            {
                if (T > 100 || T < 0)
                {
                    T = 1;
                }
                canniEf(sourceImage);
                canniEdg(sourceImage);
            }
            else
            {
                textBox3.Clear();
                textBox4.Clear();
                MessageBox.Show("Загрузите изображение");
            }
        }
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && (e.KeyChar < 48 || e.KeyChar > 57))
                e.Handled = true;
        }
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != 8 && (e.KeyChar < 48 || e.KeyChar > 57))
                e.Handled = true;
        }
    }
}
