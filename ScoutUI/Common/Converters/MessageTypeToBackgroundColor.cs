using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
  
    public class MessageTypeToBackgroundColor : BaseValueConverter
    {
     
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                switch ((MessageType)value)
                {
                    case MessageType.Normal:
                        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2B76C5"));
                    case MessageType.System:
                        return new SolidColorBrush(Colors.Red);
                    case MessageType.Warning:
                        return new SolidColorBrush(Colors.Yellow);
                }
            }
            return new SolidColorBrush(Colors.Transparent);
        }

      
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}