using System.Windows;
using System.Windows.Media;
using ScoutUtilities.Interfaces;

namespace ScoutUtilities.Services
{
    public class SolidColorBrushService : ISolidColorBrushService
    {
        public SolidColorBrush GetBrushForColor(string color)
        {
            return (SolidColorBrush) Application.Current.Resources[color];
        }
    }
}