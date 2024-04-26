using ScoutUtilities.Common;

namespace ScoutDomains
{
    public class HistogramChartItem : BaseNotifyPropertyChanged
    {
        public double GrayLevel
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public double Pixel
        {
            get { return GetProperty<double>(); }
            set { SetProperty(value); }
        }

        public HistogramChartItem(double grayLevel, double pixel)
        {
            GrayLevel = grayLevel;
            Pixel = pixel;
        }
    }
}