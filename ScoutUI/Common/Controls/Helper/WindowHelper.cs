using System.Windows;


namespace ScoutUI.Common.Controls.Helper
{
   
    public class WindowHelper
    {
       
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached("DialogResult", typeof(bool?), typeof(WindowHelper),
                new PropertyMetadata(DialogResultChanged));


        private static void DialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var window = d as Window;
            if (window != null && (bool?)e.NewValue == true)
                window.Close();
        }

     
        public static void SetDialogResult(Window target, bool? value)
        {
            target.SetValue(DialogResultProperty, value);
        }
    }
}
