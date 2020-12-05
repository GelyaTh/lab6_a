using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Lab6_AOCI
{
    public partial class Form1 : Form
    {
        private VideoCapture capture;
        MotionDetection motionDetector;
        string fileName;

        public Form1()
        {
            InitializeComponent();
            motionDetector = new MotionDetection();
        }

        //старт видео
        private void button1_Click(object sender, EventArgs e)
        {
            if (capture != null)
                timer1.Enabled = true;
        }


        private void ProcessFrame(Mat frame)
        {
            Image<Bgr, byte> srcImage = frame.ToImage<Bgr, byte>().Resize(640, 480, Inter.Linear);
            Image<Bgr, byte> resultImage = motionDetector.GetDetectedAreas(srcImage);
            imageBox1.Image = srcImage;
            imageBox2.Image = resultImage;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.Filter = "Файлы видео (*.webm,  *.mp4)  |  *.webm;  *.mp4";
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла

            if (result == DialogResult.OK) // открытие выбранного файла
            {
                fileName = openFileDialog.FileName;
                motionDetector.UpdateBackgroundSubtractor();
                capture = new VideoCapture(fileName);
                timer1.Interval = (int)(1000 / capture.GetCaptureProperty(CapProp.Fps));
                timer1.Enabled = true;
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            var frame = capture.QueryFrame();
            if (!frame.IsEmpty)
            {
                ProcessFrame(frame);
            }
            else
            {
                timer1.Enabled = false;
                MessageBox.Show("Конец :с");
            }
        }

        //пауза
        private void button3_Click(object sender, EventArgs e)
        {
            timer1.Enabled = false;
        }

        //рестарт видео
        private void button4_Click(object sender, EventArgs e)
        {
            if (fileName != null)
            {
                motionDetector.UpdateBackgroundSubtractor();
                capture = new VideoCapture(fileName);
                timer1.Enabled = true;
            }
        }
    }
}
