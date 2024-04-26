using System.Windows.Controls;
using ScoutUtilities.Enums;
using System.Windows;

namespace ScoutUI.Common.Controls
{
    public class SensorButton : Button
    {
        public eSensorStatus SensorStatus
        {
            get { return (eSensorStatus) GetValue(SensorStatusProperty); }
            set { SetValue(SensorStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SensorStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SensorStatusProperty =
            DependencyProperty.Register("SensorStatus", typeof(eSensorStatus), typeof(SensorButton),
                new PropertyMetadata(eSensorStatus.ssStateUnknown));
    }
}