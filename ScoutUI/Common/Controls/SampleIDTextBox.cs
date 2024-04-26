using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScoutUI.Common.Controls
{
  
    public class SampleIDTextBox : TextBox
    {
      
        protected override void OnPreviewKeyDown(KeyEventArgs e) { }

  
        static SampleIDTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SampleIDTextBox),
                new FrameworkPropertyMetadata(typeof(SampleIDTextBox)));
        }

        public object Watermark
        {
            get { return (object) GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

    
        public static readonly DependencyProperty WatermarkProperty = DependencyProperty.Register(
            "Watermark", typeof(object), typeof(SampleIDTextBox),
            new UIPropertyMetadata(null));
    }
}