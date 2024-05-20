using ApiProxies.Generic;
using HawkeyeCoreAPI;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.Reports;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Reports;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using HawkeyeCoreAPI.Facade;

namespace ScoutViewModels.ViewModels.Reports
{
    public class CellTypesResultsReportViewModel : BaseViewModel
    {
        #region Constructor

        public CellTypesResultsReportViewModel() : base()
        {
            ResultModel = new ResultModel();
            SettingsModel = new SettingsModel();
            Initialize();
        }

        public CellTypesResultsReportViewModel(SettingsModel settingsModel, MainWindowViewModel mainWindowVm, ResultModel resultModel) : base()
        {
            
            ResultModel = resultModel;
            SettingsModel = settingsModel;
            Initialize();
        }

        private void Initialize()
        {
            OnCellTypeSelectionChanged = new RelayCommand(OnCellTypeSelectionChangedExecute, null);
            PrintCommand = new RelayCommand(PrintExecute, null);
            UserList = new List<UserDomain>(SettingsModel.UserModel.UserList);
            SetUserList();
            PrintTitle = ApplicationVersion;
            IsPrintCommandEnabled = true;
            IsUserListEnable = !LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eNormal);

            _ctqcSubscriber = MessageBus.Default.Subscribe<Notification>(OnCellTypesQualityControlsUpdated);
        }

        protected override void DisposeUnmanaged()
        {
            MessageBus.Default.UnSubscribe(ref _ctqcSubscriber);
            base.DisposeUnmanaged();
        }

        #endregion Constructor

        #region Private Properties

        private UserDomain _selectedUser;
        private Subscription<Notification> _ctqcSubscriber;

        #endregion Private Properties

        #region Public Properties

        public ICommand OnCellTypeSelectionChanged { get; set; }

        public ICommand PrintCommand { get; set; }

        public ResultModel ResultModel { get; set; }

        public SettingsModel SettingsModel;

        public string PrintTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public string Comments
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsUserListEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public List<UserDomain> UserList
        {
            get { return GetProperty<List<UserDomain>>(); }
            set { SetProperty(value); }
        }

        public UserDomain SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (_selectedUser == value)
                    return;
                _selectedUser = value;
                NotifyPropertyChanged(nameof(SelectedUser));
                if (_selectedUser != null)
                    OnChangedSelectedUser(value);
            }
        }

        public List<CellTypeDomain> CellList
        {
            get { return GetProperty<List<CellTypeDomain>>(); }
            set { SetProperty(value); }
        }

        public CellTypeDomain SelectedCell
        {
            get { return GetProperty<CellTypeDomain>(); }
            set { SetProperty(value); }
        }

        public bool IsPrintCommandEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion Public Properties

        #region Private Methods

        private void OnCellTypeSelectionChangedExecute(object obj)
        {
            try
            {
                IsPrintCommandEnabled = false;
                foreach (var cellTypeObj in CellList)
                {
                    if (cellTypeObj.IsCellSelected)
                    {
                        IsPrintCommandEnabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ON_CELLTYPE_SELECTION"));
            }
        }

        private void OnExecute()
        {
            try
            {
                if (CellList != null && CellList.Count > 0)
                {
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        var cellTypeReportViewModel = new CellTypesReportViewModel(SelectedUser.UserID, PrintTitle, Comments, CellList);
                        cellTypeReportViewModel.LoadReport();
                        PublishReportProgressIndication(false);
                        ReportEventBus.CellTypesReport(this, cellTypeReportViewModel);
                    });
                }
                else
                {
                    PublishReportProgressIndication(false);
                    DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_Report_NoDataFound"));
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ADMIN_BIOPROCESS_PRINT"));
            }
        }

        private async void PrintExecute()
        {
            PublishReportProgressIndication(true);
            try
            {
                await Task.Run(() =>
                {
                    OnExecute();
                });
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEXPORT"));
                PublishReportProgressIndication(false);
            }
        }

        private void PublishReportProgressIndication(bool value)
        {
            MessageBus.Default.Publish(new ReportProgressIndicationDomain { 
                IsProgressIndicationVisible = value 
            });
        }

        private void SetUserList()
        {
            switch (LoggedInUser.CurrentUserId)
            {
                case ApplicationConstants.ServiceUser:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceUser });
                    SelectedUser = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceUser));
                    break;
                case ApplicationConstants.SilentAdmin:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.SilentAdmin });
                    SelectedUser = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.SilentAdmin));
                    break;
                case ApplicationConstants.AutomationClient:
                    UserList.Add(new UserDomain { UserID = ApplicationConstants.AutomationClient });
                    SelectedUser = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.AutomationClient));
                    break;
                //// Leave for future reference or use
                //case ApplicationConstants.ServiceAdmin:
                //    UserList.Add(new UserDomain { UserID = ApplicationConstants.ServiceAdmin });
                //    SelectedUser = UserList.FirstOrDefault(a => a.UserID.Equals(ApplicationConstants.ServiceAdmin));
                //    break;
                default:
                    SelectedUser = UserList.FirstOrDefault(a => a.UserID.Equals(LoggedInUser.CurrentUserId));
                    break;

            }
        }

        private void OnCellTypesQualityControlsUpdated(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;

            if (msg.Token.Equals(MessageToken.CellTypesUpdated) || msg.Token.Equals(MessageToken.QualityControlsUpdated))
            {
                OnChangedSelectedUser(LoggedInUser.CurrentUser);
            }
        }

        private void OnChangedSelectedUser(UserDomain selectedUser)
        {
            if (selectedUser?.UserID == ApplicationConstants.ServiceUser ||
                selectedUser?.UserID == ApplicationConstants.SilentAdmin)
            {
                var allCellTypes = CellTypeFacade.Instance.GetAllCellTypes_BECall();
                CellList = allCellTypes;
            }
            else if (LoggedInUser.CurrentUserRoleId.Equals(UserPermissionLevel.eNormal))
            {
                // When no user is logged in the current user role is normal 
                if (LoggedInUser.CurrentUser?.AssignedCellTypes == null)
                {
                    CellList = new List<CellTypeDomain>();
                }
                else
                {
                    CellList = LoggedInUser.CurrentUser.AssignedCellTypes.ToList();
                }
            }
            else
            {
                CellList = CellTypeFacade.Instance.GetAllowedCellTypes_BECall(selectedUser.UserID);
            }

            SetDefaultValues();
        }

        private void SetDefaultValues()
        {
            CellList?.ForEach(cellType => { cellType.IsCellSelected = true; });
            Comments = string.Empty;
        }

        #endregion Private Methods

    }

}