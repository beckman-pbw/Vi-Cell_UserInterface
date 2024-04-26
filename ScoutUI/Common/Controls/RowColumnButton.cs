using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class RowColumnButton : Button
    {
        public bool IsRowActive
        {
            get { return (bool) GetValue(IsRowActiveProperty); }
            set { SetValue(IsRowActiveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsRowActive.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRowActiveProperty =
            DependencyProperty.Register("IsRowActive", typeof(bool), typeof(RowColumnButton),
                new PropertyMetadata(false));
    }
}