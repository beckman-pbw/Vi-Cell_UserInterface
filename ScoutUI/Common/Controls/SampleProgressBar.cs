using ScoutUtilities.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
   
    public class SampleProgressBar : Button
    {
      
        public SampleProgressStatus ProgressStatus
        {
            get { return (SampleProgressStatus) GetValue(ProgressStatusProperty); }
            set { SetValue(ProgressStatusProperty, value); }
        }

        public static readonly DependencyProperty ProgressStatusProperty =
            DependencyProperty.Register("ProgressStatus", typeof(SampleProgressStatus), typeof(SampleProgressBar),
                new PropertyMetadata(SampleProgressStatus.IsActive));
    }
}