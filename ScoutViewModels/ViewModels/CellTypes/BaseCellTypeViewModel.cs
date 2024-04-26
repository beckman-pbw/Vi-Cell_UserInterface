using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ScoutDomains;
using ScoutModels.Interfaces;
using ScoutServices.Interfaces;

namespace ScoutViewModels.ViewModels.CellTypes
{
    public class BaseCellTypeViewModel : BaseViewModel
    {
        #region Contructor

        public BaseCellTypeViewModel(UserDomain currentUser, IInstrumentStatusService instrumentStatusService,  ICellTypeManager cellTypeManager) : base(currentUser)
        {
            CurrentUser = currentUser;
            IsAdminUser = CurrentUser.RoleID == UserPermissionLevel.eAdministrator;

            _instrumentStatusService = instrumentStatusService;

            _cellTypeManager = cellTypeManager;

            DeclusterList = new ObservableCollection<eCellDeclusterSetting>(CellTypeModel.GetDeclusterList());
            if (DeclusterList.Count > 0) SelectedDecluster = DeclusterList[0];

            MixingCyclesList = new ObservableCollection<int>(GetMixingAspirationCycle());
            AspirationCyclesList = new ObservableCollection<int>(GetMixingAspirationCycle());
            AvailableCellTypes = currentUser.AssignedCellTypes.ToObservableCollection();

            _systemStatusSubscriber = _instrumentStatusService.SubscribeToSystemStatusCallback().Subscribe((OnSystemStatusChanged));
        }

        protected override void DisposeUnmanaged()
        {
            _systemStatusSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties

        private readonly IInstrumentStatusService _instrumentStatusService;
        private IDisposable _systemStatusSubscriber;
        protected UserDomain CurrentUser;
        protected SystemStatusDomain SystemStatusDomain => _instrumentStatusService.SystemStatusDom;
        protected readonly ICellTypeManager _cellTypeManager;

        public int SelectedAspirationCycle
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int SelectedMixingCycle
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public eCellDeclusterSetting SelectedDecluster
        {
            get { return GetProperty<eCellDeclusterSetting>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<eCellDeclusterSetting> DeclusterList
        {
            get { return GetProperty<ObservableCollection<eCellDeclusterSetting>>(); }
            private set { SetProperty(value); }
        }

        public ObservableCollection<int> MixingCyclesList
        {
            get { return GetProperty<ObservableCollection<int>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<int> AspirationCyclesList
        {
            get { return GetProperty<ObservableCollection<int>>(); }
            set { SetProperty(value); }
        }

        public ObservableCollection<CellTypeDomain> AvailableCellTypes
        {
            get { return GetProperty<ObservableCollection<CellTypeDomain>>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Event Handlers

        private void OnSystemStatusChanged(SystemStatusDomain systemStatusDomain)
        {
            OnSystemStatusChanged();
        }

        protected virtual void OnSystemStatusChanged()
        {
        }

        #endregion

        #region Commands

        private RelayCommand _cancelCommand;
        public virtual RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(PerformCancelCommand, null));

        private void PerformCancelCommand(object param)
        {
            try
            {
                if (param == null) return;
                CloseWindow(param);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_CELLTYPE_CLOSE_ERROR"));
            }
        }

        #endregion

        #region Public Method
        
        public  LoginResult? SecurityCheck()
        {
            LoginResult? result = LoginResult.CurrentUserLoginSuccess;

            if (IsSecurityTurnedOn)
            {
                var args = new LoginEventArgs(LoggedInUser.CurrentUserId,
                    LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp);
                result = DialogEventBus.Login(this, args);
            }

            return result;
        }

        public bool AddUserDefineCellType(string username, string password, CellTypeDomain selectedCellType)
        {
            if (!(selectedCellType.Clone() is CellTypeDomain clonedCell))
            {
                return false;
            }

            SetCellTypeValue(clonedCell);

            return _cellTypeManager.CreateCellType(username, password, clonedCell, selectedCellType.TempCellName, true);
        }

        // Deprecated
        public bool ModifyCellType(string username, string password, CellTypeDomain selectedCellType)
        {
            SetCellTypeValue(selectedCellType);
            selectedCellType.CellTypeName = selectedCellType.TempCellName;
            CellTypeModel.SpecializeAnalysisForCellType(selectedCellType.AnalysisDomain, selectedCellType.CellTypeIndex);
            var status = CellTypeFacade.Instance.Edit(username, password, selectedCellType);
            if (status.Equals(HawkeyeError.eSuccess))
            {
				var str = LanguageResourceHelper.Get("LID_StatusBar_ModifiedCellType");
                PostToMessageHub(str);
                Log.Debug(str);
                return true;
            }

            ApiHawkeyeMsgHelper.ErrorCreateCellType(status);
            return false;
        }

        public void SetCellTypeValue(CellTypeDomain selectedCellType)
        {
            selectedCellType.AnalysisDomain.MixingCycle = SelectedMixingCycle;
            selectedCellType.DeclusterDegree = SelectedDecluster;
            selectedCellType.AspirationCycles = SelectedAspirationCycle;
        }

        public void SetSelectedCellValues(CellTypeDomain selectedCellFromList)
        {
            SelectedMixingCycle = selectedCellFromList.AnalysisDomain.MixingCycle;
            SelectedAspirationCycle = selectedCellFromList.AspirationCycles;
            SelectedDecluster = selectedCellFromList.DeclusterDegree;
        }

        #endregion

        #region Private Methods

        private List<int> GetMixingAspirationCycle()
        {
            var cycleList = new List<int>();
            for (int i = 1; i <= 10; i++)
            {
                cycleList.Add(i);
            }

            return cycleList;
        }

        #endregion
    }
}