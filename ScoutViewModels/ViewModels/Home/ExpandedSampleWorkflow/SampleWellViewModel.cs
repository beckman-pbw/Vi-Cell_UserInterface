using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System.Windows;
using System.Windows.Media;
using ScoutUtilities.Interfaces;

namespace ScoutViewModels.ViewModels.ExpandedSampleWorkflow
{
    public class SampleWellViewModel : BaseViewModel
    {
        #region Constructor

        public SampleWellViewModel(RowPosition rowPosition, byte column, ISolidColorBrushService colorBrushService)
        {
            SamplePosition = new SamplePosition(rowPosition, column);
            _colorBrushService = colorBrushService;
        }

        #endregion

        #region Properties & Fields

        private ISolidColorBrushService _colorBrushService;

        public SampleViewModel Sample
        {
            get { return GetProperty<SampleViewModel>(); }
            set { SetProperty(value); }
        }

        public SamplePosition SamplePosition
        {
            get { return GetProperty<SamplePosition>(); }
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

        public SampleWellState WellState
        {
            get { return GetProperty<SampleWellState>(); }
            set
            {
                SetProperty(value);
                switch (value)
                {
                    case SampleWellState.Available:
                        Sample = null;
                        FillColor = _colorBrushService.GetBrushForColor("SampleWell_Available_Color");
                        StrokeColor = _colorBrushService.GetBrushForColor("SampleWell_Available_Border");
                        IsEnabled = true;
                        IsChecked = false;
                        break;
                    case SampleWellState.Processing:
                        FillColor = _colorBrushService.GetBrushForColor("SampleWell_Processing_Color");
                        StrokeColor = _colorBrushService.GetBrushForColor("SampleWell_Processing_Border");
                        IsEnabled = false;
                        IsChecked = false;
                        break;
                    case SampleWellState.UsedInCurrentSet:
                        FillColor = _colorBrushService.GetBrushForColor("SampleWell_UsedInCurrentSet_Color");
                        StrokeColor = _colorBrushService.GetBrushForColor("SampleWell_UsedInCurrentSet_Border");
                        IsEnabled = true;
                        IsChecked = true;
                        break;
                    case SampleWellState.UsedInAnotherSet:
                        FillColor = _colorBrushService.GetBrushForColor("SampleWell_UsedInAnotherSet_Color");
                        StrokeColor = _colorBrushService.GetBrushForColor("SampleWell_UsedInAnotherSet_Border");
                        IsEnabled = false;
                        IsChecked = true;
                        break;
                    case SampleWellState.Blocked:
                        FillColor = _colorBrushService.GetBrushForColor("SampleWell_Blocked_Color");
                        StrokeColor = _colorBrushService.GetBrushForColor("SampleWell_Blocked_Border");
                        IsEnabled = false;
                        IsChecked = true;
                        break;
                }
            }
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return SamplePosition.ToString();
        }

        #endregion
    }
}