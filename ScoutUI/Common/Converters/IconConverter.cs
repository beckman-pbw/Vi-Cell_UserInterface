using System;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace ScoutUI.Common.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapSource))]
    public class IconConverter : BaseValueConverter
    {
        public override object Convert(object value, Type type, object parameter, CultureInfo culture)
        {
            if (value == null || (MessageBoxImage)value == MessageBoxImage.None) return null;

            Icon icon = (Icon)typeof(SystemIcons).GetProperty(value.ToString(), BindingFlags.Public | BindingFlags.Static).GetValue(null, null);
            BitmapSource bs = Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            return bs;
        }

        public override object ConvertBack(object value, Type type, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
