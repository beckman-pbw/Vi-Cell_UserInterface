using ScoutDomains.Common;
using ScoutUtilities.Enums;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
  
    public class EditSampleID : TextBox
    {
       
        protected override void OnGotFocus(RoutedEventArgs e)
        {
            TextBox tb = e.OriginalSource as TextBox;
            base.OnGotFocus(e);
            var sample = DataContext as SampleDomain;
            Text = (sample == null) ? "" : sample.SampleName;
            if (tb != null)
            {
                tb.CaretIndex = Text.Length;
            }
        }

      
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            BufferName = Text;
            Text = "";
            base.OnLostFocus(e);
        }

        private string _bufferName;        
        private string BufferName
        {
            get { return _bufferName; }
            set
            {
                _bufferName = value;
                UpdateSampleId(_bufferName);
            }
        }

      
        private void UpdateSampleId(string newName)
        {
            // Cases:
            //  No data linkage: No-op
            //  Sample row not enabled / Sample not defined: SampleID is blanked entirely
            //  NewName is empty: SampleID set from default name and position
            //  NewName is not empty: SampleID set from NewName and position

            var sample = DataContext as SampleDomain;
            if (sample == null)
                return;

            if (!sample.IsEnabled || sample.SampleStatusColor != SampleStatusColor.Defined)
                SampleId = string.Empty;
            else
                SampleId =  (string.IsNullOrWhiteSpace(newName) ? DefaultSampleId : newName) + "." + SamplePosition;
        }

      
        public string SampleName => (string) GetValue(SampleNameProperty);

        public static readonly DependencyProperty SampleNameProperty =
            DependencyProperty.Register("SampleName", typeof(string), typeof(EditSampleID), new PropertyMetadata(""));

     
        private static void IsSampleNamePropertyChange(EditSampleID editSampleID, string newValue, string oldValue)
        {
            var sample = editSampleID.DataContext as SampleDomain;
            if (oldValue != null && newValue != null)
            {
                if (oldValue.Length > 0 && newValue.Length == 0)
                {
                    editSampleID.SampleId = editSampleID.DefaultSampleId + "." + editSampleID.SamplePosition;
                }
                else if (!string.IsNullOrEmpty(editSampleID.SampleName))
                {
                    editSampleID.SampleId = editSampleID.SampleName + "." + editSampleID.SamplePosition;
                }
                else
                {
                    if (sample != null && !sample.IsEnabled)
                    {
                        editSampleID.SampleId = string.Empty;
                    }
                }
            }
            else
            {
                if (sample == null)
                {
                    return;
                }
                if (sample.SampleStatusColor == SampleStatusColor.Defined)
                {
                    editSampleID.SampleId = editSampleID.SampleName + "." + editSampleID.SamplePosition;
                }
                else
                {
                    editSampleID.SampleId = string.Empty;
                }
            }
        }

        public string SampleId
        {
            get { return (string) GetValue(SampleIdProperty); }
            set { SetValue(SampleIdProperty, value); }
        }

        public static readonly DependencyProperty SampleIdProperty =
            DependencyProperty.Register("SampleId", typeof(string), typeof(EditSampleID), new PropertyMetadata(""));

        public string SamplePosition
        {
            get { return (string) GetValue(SamplePositionProperty); }
            set { SetValue(SamplePositionProperty, value); }
        }

        public static readonly DependencyProperty SamplePositionProperty =
            DependencyProperty.Register("SamplePosition", typeof(string), typeof(EditSampleID), new PropertyMetadata(""));

        public string DefaultSampleId
        {
            get { return (string) GetValue(DefaultSampleIdProperty); }
            set { SetValue(DefaultSampleIdProperty, value); }
        }

      
        public static readonly DependencyProperty DefaultSampleIdProperty =
            DependencyProperty.Register("DefaultSampleId", typeof(string), typeof(EditSampleID),
                new PropertyMetadata(null));
    }
}