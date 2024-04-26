using System;
using DrawPoint = System.Drawing.Point;
using Image = System.Windows.Controls.Image;
using WinPoint = System.Windows.Point;

namespace ScoutModels.Service
{
    public class ImageService
    {
        public static DrawPoint GetClickLocation(Image image, WinPoint mousePoint)
        {
            var inverseScalingX = image.Source.Width / image.Width;
            var inverseScalingY = image.Source.Height / image.Height;
            var point = new DrawPoint(Convert.ToInt32(inverseScalingX * mousePoint.X), Convert.ToInt32(inverseScalingY * mousePoint.Y));
            return point;
        }
    }
}