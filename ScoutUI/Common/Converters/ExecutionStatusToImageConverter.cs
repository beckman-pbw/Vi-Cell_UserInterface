using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class ExecutionStatusToImageConverter : BaseValueConverter
    {
       
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imagePath = "/Images/Empty.png";
            if (value != null)
            {
                var result = (ExecutionStatus) value;
                switch (result)
                {
                    case ExecutionStatus.Running:
                        imagePath = "/Images/RunningSample.png";
                        break;
                    case ExecutionStatus.Pause:
                        imagePath = "/Images/PauseSample.png";
                        break;
                    case ExecutionStatus.Skip:
                        imagePath = "/Images/SkipSample.png";
                        break;
                    case ExecutionStatus.Error:
                        imagePath = "/Images/ErrorSample.png";
                        break;
                    default:
                        break;
                }
            }

            return imagePath;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}