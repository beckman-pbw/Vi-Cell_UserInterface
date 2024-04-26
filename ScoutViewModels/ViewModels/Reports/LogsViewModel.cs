using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutViewModels.Interfaces;
using System;
using System.Windows.Controls;

namespace ScoutViewModels.ViewModels.Reports
{
    public class LogsViewModel : BaseViewModel
    {
        public LogsViewModel(IScoutViewModelFactory viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        #region Properties & Fields

        private readonly IScoutViewModelFactory _viewModelFactory;
        
        public ReportsDomain SelectedLog
        {
            get { return GetProperty<ReportsDomain>(); }
            set
            {
                if (value == null)
                    return;

                SetProperty(value);
                OnSelectedLogChanged(value.ReportId);
            }
        }

        private ContentControl _logContentControl;
        public ContentControl LogContentControl
        {
            get { return _logContentControl ?? (_logContentControl = new ContentControl()); }
            set
            {
                _logContentControl = value;
                NotifyPropertyChanged(nameof(LogContentControl));
            }
        }
        
        #endregion
        
        #region Private Methods

        private void OnSelectedLogChanged(int reportId)
        {
            try
            {
                LogContentControl.Content = null;
                if (reportId == 0)
                    return;

                switch (reportId)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                        // load the content for the Log View
                        LogContentControl.Content = _viewModelFactory.CreateLogPanelViewModel(reportId);
                        break;
                    case 5:
                        // load the content for the scheduled exports View
                        LogContentControl.Content = _viewModelFactory.CreateAuditLogScheduledExportsViewModel();
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_REPORT_SELECTION_CHANGE"));
            }
        }

        #endregion
    }
}