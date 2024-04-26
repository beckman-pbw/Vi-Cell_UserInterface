using System.Windows.Media;

namespace ScoutUtilities.Interfaces
{
    public interface ISolidColorBrushService
    {
        SolidColorBrush GetBrushForColor(string color);
    }
}