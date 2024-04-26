using System;
using AForge.Imaging;
using ScoutDomains;
using ScoutUtilities.ImageUtilities;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ScoutUtilities.Common;

namespace ScoutViewModels.Common.Helper
{
    public class HistogramUtilityHelper
    {
        public ObservableCollection<HistogramChartItem> GetChartItems()
        {
	        var DefaultImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ApplicationConstants.TargetFolderName);
            var imageName = ApplicationConstants.ImageFilenameForHistogram + ApplicationConstants.ImageFileExtension;
            var imagePath = Path.Combine(DefaultImageDirectory, imageName);

            var itemCollection = new ObservableCollection<HistogramChartItem>();
            using (var bmp = new Bitmap(imagePath))
            {
                var hslStatistics = new ImageStatisticsHSL(bmp);
                var i = 0;
                foreach (var item in hslStatistics.Luminance.Values)
                {
                    itemCollection.Add(new HistogramChartItem(i, item));
                    i++;
                }
            }

            return itemCollection;
        }

        public ObservableCollection<HistogramChartItem> GetChartItemsWithStream(ImageSource source)
        {
            var itemCollection = new ObservableCollection<HistogramChartItem>();
            var bitmapImage = ImageUtility.BitmapSourceToBitmap((CachedBitmap)source);
            var hslStatistics = new ImageStatisticsHSL(bitmapImage);
            var i = 0;
            foreach (var item in hslStatistics.Luminance.Values)
            {
                itemCollection.Add(new HistogramChartItem(i, item));
                i++;
            }

            return itemCollection;
        }
    }
}