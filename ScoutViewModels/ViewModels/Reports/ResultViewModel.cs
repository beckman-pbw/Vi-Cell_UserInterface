using ScoutDomains;
using ScoutViewModels.ViewModels.Reports;
using System.Windows.Controls;
using ScoutViewModels.Interfaces;

namespace ScoutViewModels.ViewModels
{
    public class ResultViewModel : ScoutModels.Common.BaseDisposableNotifyPropertyChanged
    {
        public ResultViewModel(IScoutViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        protected override void DisposeUnmanaged()
        {
            ((BaseViewModel)ReportsResultsContentControl.Content)?.Dispose();
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly IScoutViewModelFactory _viewModelFactory;
        
        public ReportsDomain SelectedResult
        {
            get { return GetProperty<ReportsDomain>(); }
            set
            {
                if (value == null)
                    return;

                SetProperty(value);
                SwitchToResultContent(SelectedResult.ReportId);
            }
        }

        private ContentControl _reportsResultsContentControl;
        public ContentControl ReportsResultsContentControl
        {
            get { return _reportsResultsContentControl ?? (_reportsResultsContentControl = new ContentControl()); }
            set
            {
                _reportsResultsContentControl = value;
                NotifyPropertyChanged(nameof(ReportsResultsContentControl));
            }
        }

        #endregion

        private void SwitchToResultContent(int reportId)
        {
            ((BaseViewModel)ReportsResultsContentControl.Content)?.Dispose();
            ReportsResultsContentControl.Content = null;
            if (reportId == 0)
                return;
            switch (reportId)
            {
                case 1:
                    ReportsResultsContentControl.Content = new CompletedRunSummaryResultViewModel();
                    break;
                case 2:
                    ReportsResultsContentControl.Content = _viewModelFactory.CreateResultsRunResultsViewModel();
                    break;
                case 3:
                    // Bioprocess -- not used
                    break;
                case 4:
                    ReportsResultsContentControl.Content = new ResultsQualityControlViewModel(_viewModelFactory);
                    break;
                case 5:
                    ReportsResultsContentControl.Content = new CellTypesResultsReportViewModel();
                    break;
                case 6:
                    ReportsResultsContentControl.Content = _viewModelFactory.CreateInstrumentStatusResultViewModel();
                    break;
                case 7:
                    ReportsResultsContentControl.Content = _viewModelFactory.CreateScheduledExportsViewModel();
                    break;
            }
        }
    }
}