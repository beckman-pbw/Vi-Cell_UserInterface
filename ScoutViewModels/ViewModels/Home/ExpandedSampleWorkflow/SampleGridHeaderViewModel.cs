using System.Windows;
using System.Windows.Media;
using ScoutUtilities.Interfaces;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class SampleGridHeaderViewModel : BaseViewModel
    {
        public SampleGridHeaderViewModel(string label, ISolidColorBrushService colorBrushService)
        {
            Label = label;
            IsEnabled = true;
            IsChecked = false;
            StrokeColor = colorBrushService.GetBrushForColor("TitleBar_Background");
            FillColor = colorBrushService.GetBrushForColor("TitleBar_Background");
        }

        #region Properties & Fields

        public string Label
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsChecked
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public Brush FillColor
        {
            get { return GetProperty<Brush>(); }
            private set { SetProperty(value); }
        }

        public Brush StrokeColor
        {
            get { return GetProperty<Brush>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return Label;
        }

        #endregion
    }
}