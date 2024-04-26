using ApiProxies.Generic;
using ScoutDomains;
using ScoutDomains.Reports;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Reports;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.Interfaces;
using System;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Reports
{
    public class ReportsPanelViewModel : BaseViewModel
    {
        public ReportsPanelViewModel(IScoutViewModelFactory viewModelFactory) : base()
        {
            _viewModelFactory = viewModelFactory;
            _reportsModel = new ReportsModel();
            IsLogAccessible = true;
            SelectedTabItem = 0;
            EventSubscription();
            SetAdminAccess();
        }

        protected override void DisposeUnmanaged()
        {
            _resultViewModel?.Dispose();
            MessageBus.Default.UnSubscribe(ref _eventSubscription);
            base.DisposeUnmanaged();
        }

        #region Properties & Fields

        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly ReportsModel _reportsModel;
        private Subscription<ReportProgressIndicationDomain> _eventSubscription;

        public bool IsProgressIndicationVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsLogAccessible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public IList<ReportsDomain> LogsNameList
        {
            get { return _reportsModel.LogsNameList; }
            set
            {
                _reportsModel.LogsNameList = value;
                NotifyPropertyChanged(nameof(LogsNameList));
            }
        }

        public IList<ReportsDomain> ReportNameList
        {
            get { return _reportsModel.ReportNameList; }
            set
            {
                _reportsModel.ReportNameList = value;
                NotifyPropertyChanged(nameof(ReportNameList));
            }
        }

        public int SelectedTabItem
        {
            get { return GetProperty<int>(); }
            set
            {
                SetProperty(value);
                SwitchAdminCommonListContent(value);
            }
        }
        
        private ResultViewModel _resultViewModel;
        public ResultViewModel ResultViewModel
        {
            get { return _resultViewModel ?? (_resultViewModel = _viewModelFactory.CreateResultViewModel()); }
            set
            {
                _resultViewModel = value;
                NotifyPropertyChanged(nameof(ResultViewModel));
            }
        }

        private LogsViewModel _logViewModel;
        public LogsViewModel LogViewModel
        {
            get { return _logViewModel ?? (_logViewModel = _viewModelFactory.CreateLogsViewModel()); }
            set
            {
                _logViewModel = value;
                NotifyPropertyChanged(nameof(LogViewModel));
            }
        }

        #endregion

        #region Private Methods

        private void SwitchAdminCommonListContent(int value)
        {
            switch (value)
            {
                case 0:
                    if (ReportNameList.Count > 0)
                        ResultViewModel.SelectedResult = ReportNameList[0];
                    break;
                case 1:
                    if (LogsNameList.Count > 0)
                        LogViewModel.SelectedLog = LogsNameList[0];
                    break;
            }
        }
        
        private void EventSubscription()
        {
            _eventSubscription = MessageBus.Default.Subscribe<ReportProgressIndicationDomain>(domain =>
            {
                if (domain != null)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        IsProgressIndicationVisible = domain.IsProgressIndicationVisible;
                    });
                }
            });
        }

        private void SetAdminAccess()
        {
            if (LoggedInUser.CurrentUser?.RoleID != null && LoggedInUser.CurrentUser.RoleID.Equals(UserPermissionLevel.eNormal))
                IsLogAccessible = false;
        }

        #endregion
    }
}