using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;

namespace Lab6_AOCI
{
    class MotionDetection
    {
        private Image<Gray, byte> backGround;
        //public Image<Gray, byte> BackGround { get => backGround; set => backGround = value; }
        private BackgroundSubtractorMOG2 subtractor;

        public MotionDetection()
        {

        }

        public void UpdateBackgroundSubtractor()
        {
            subtractor = new BackgroundSubtractorMOG2(1000, 32, true);
        }

        public Image<Bgr, byte> GetDetectedAreas(Image<Bgr, byte> frame)
        {
            //Image<Gray, byte> foregroundMask = GetMask(frame.Convert<Gray, byte>());
            Image<Gray, byte> foregroundMask = GetMaskFromDiff(frame.Convert<Gray, byte>());
            //return foregroundMask.Convert<Bgr, byte>();
            return GetContours(frame, foregroundMask);
        }

        private Image<Gray, byte> GetMask(Image<Gray, byte> frame)
        {
            var foregroundMask = frame.CopyBlank();
            subtractor.Apply(frame, foregroundMask);
            return FilterMask(foregroundMask);
        }   

        private Image<Gray, byte> GetMaskFromDiff(Image<Gray, byte> frame)
        {
            if (backGround == null)
            {
                backGround = frame.Copy();
            }
            var diff = backGround.AbsDiff(frame);
            diff = diff.Erode(3);
            diff = diff.Dilate(4);
            /*diff.Erode(3); 
            diff.Dilate(4);*/
           backGround = frame.Copy();
            return diff;
        }

        private Image<Bgr, byte> GetContours(Image<Bgr, byte> frame, Image<Gray, byte> mask)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.FindContours(mask, contours, null, RetrType.External, ChainApproxMethod.ChainApproxTc89L1);
            var output = frame.Copy();
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i], false) > 700) //игнорирование маленьких контуров
                {
                    Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);
                    output.Draw(rect, new Bgr(Color.GreenYellow), 2);  
                }
            }
            return output;
        }

        private Image<Gray, byte> FilterMask(Image<Gray, byte> mask)
        {
            var anchor = new Point(-1, -1);
            var borderValue = new MCvScalar(1);

            // создание структурного элемента заданного размера и формы для морфологических операций
            var kernel = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(3, 3), anchor);

            // заполнение небольших тёмных областей
            var closing = mask.MorphologyEx(MorphOp.Close, kernel, anchor, 1, BorderType.Default,
            borderValue);

            // удаление шумов
            var opening = closing.MorphologyEx(MorphOp.Open, kernel, anchor, 1, BorderType.Default,
            borderValue);

            // расширение для слияния небольших смежных областей
            var dilation = opening.Dilate(7);

            // пороговое преобразование для удаления теней
            var threshold = dilation.ThresholdBinary(new Gray(240), new Gray(255));

            return threshold;
        }
    }
}
