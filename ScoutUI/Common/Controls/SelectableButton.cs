using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class SelectableButton : Button
    {
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register(nameof(IsSelected), typeof(bool),
                typeof(SelectableButton), new PropertyMetadata(false));

    }
}