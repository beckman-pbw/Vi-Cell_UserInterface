using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class WatermarkedTextBox : TextBox
    {
        public int Watermark
        {
            get { return (int)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(nameof(Watermark),
            typeof(string), typeof(WatermarkedTextBox), new PropertyMetadata(string.Empty));

    }
}