using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
   
    public class ActiveButton : Button
    {
       
        public bool IsActive
        {
            get { return (bool) GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

     
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ActiveButton), new PropertyMetadata(false));
    }
}
