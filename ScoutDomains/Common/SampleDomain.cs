using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Windows.Media;
using ScoutUtilities.Common;

namespace ScoutDomains.Common
{
    public class SampleDomain : BaseSampleDomain
    {
        #region Constructor

        public SampleDomain()
        {
            Color = Brushes.Transparent;
        }

        #endregion

        #region Properties & Fields

        public bool IsExportEachSampleActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value == false) IsExportPDFSelected = false;
            }
        }

        public bool IsExportPDFSelected
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsAppendResultExport
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string ExportPathForEachSample
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string AppendResultExport
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string ExportFileName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public int NthImage
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int Tag
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string BpQcName
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public ExecutionStatus ExecutionStatus
        {
            get { return GetProperty<ExecutionStatus>(); }
            set { SetProperty(value); }
        }

        public string PlayStatusImage
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public SampleStatusColor SampleStatusColor
        {
            get { return GetProperty<SampleStatusColor>(); }
            set { SetProperty(value); }
        }

        public RunSampleProgressIndicator RunSampleProgress
        {
            get { return GetProperty<RunSampleProgressIndicator>(); }
            set { SetProperty(value); }
        }

        public bool CurrentSelectedSample
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public SolidColorBrush Color
        {
            get { return GetProperty<SolidColorBrush>(); }
            set { SetProperty(value); }
        }

        public bool IsCellTypeWashDilutionVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public CarouselButtonState ButtonState
        {
            get { return GetProperty<CarouselButtonState>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Public Methods

        public string SetCarouselPositionToSampleId(SamplePosition samplePosition)
        {
            string sampleRowPosition;

            if (samplePosition.Column > 9)
            {
                sampleRowPosition = "0" + samplePosition.Column;
            }
            else
            {
                sampleRowPosition = "00" + samplePosition.Column;
            }

            return sampleRowPosition;
        }

        public void Clear()
        {
            PlayStatusImage = string.Empty;
            SampleID = string.Empty;
            SelectedDilution = string.Empty;
            SelectedWash = 0;
            Comment = string.Empty;
            SampleStatusColor = SampleStatusColor.Empty;
            SelectedCellTypeQualityControlGroup = new CellTypeQualityControlGroupDomain();
            IsRunning = false;
            RunSampleProgress = RunSampleProgressIndicator.eNotProcessed;
            ExecutionStatus = ExecutionStatus.Default;
            IsEnabled = false;
            Color = Brushes.Transparent;
            IsCellTypeWashDilutionVisible = false;
        }

        #endregion
    }
}
