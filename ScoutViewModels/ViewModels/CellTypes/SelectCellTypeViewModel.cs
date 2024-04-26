using ScoutDomains.Analysis;
using ScoutModels;
using ScoutModels.Service;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutViewModels.ViewModels.Dialogs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Facade;
using ScoutUtilities.Helper;
using ScoutServices.Interfaces;
using System;
using ScoutServices.Enums;

namespace ScoutViewModels.ViewModels.CellTypes
{
    public class SelectCellTypeViewModel : BaseDialogViewModel
    {
        #region Constructor 

        public SelectCellTypeViewModel(SelectCellTypeEventArgs args, System.Windows.Window parentWindow, ILockManager lockManager) : base(args, parentWindow)
        {
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
            DialogTitle = args.Header;
            IsDiagnosticAvailable = args.IsDiagnosticAvailable;

            AllCellTypesList = LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection();
            AvailableAnalyses = AnalysisModel.GetAnalysisDomains(IsDiagnosticAvailable);

            if (IsDiagnosticAvailable)
            {
                AcceptButtonIcon = "/Images/Tick.png";
                using (var tempOpticsManualServiceModel = new OpticsManualServiceModel())
                {
                    var tempCellTypes = tempOpticsManualServiceModel.svc_GetTemporaryCellType();
                    SelectedCellType = tempCellTypes.Any()
                        ? tempCellTypes.First()
                        : AllCellTypesList.First();
                }
            }
            else
            {
                AcceptButtonIcon = "/Images/Cell.png";
                AvailableAnalyses = AnalysisModel.GetAnalysisDomains(IsDiagnosticAvailable);

                if (args.SelectedCellTypeDomain != null && args.SelectedCellTypeDomain is CellTypeDomain cellType)
                {
                    SelectedCellType = AllCellTypesList.FirstOrDefault(cell => cell.TempCellName.Equals(cellType.TempCellName));
                }

                if (SelectedCellType == null)
                {
                    SelectedCellType = AllCellTypesList.FirstOrDefault();
                }
            }
        }

        protected override void DisposeUnmanaged()
        {
            _lockStateSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private readonly ILockManager _lockManager;
        private IDisposable _lockStateSubscriber;

        public ObservableCollection<CellTypeDomain> AllCellTypesList
        {
            get
            {
                var item = GetProperty<ObservableCollection<CellTypeDomain>>();
                if (item != null) return item;
                SetProperty(LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection());
                return GetProperty<ObservableCollection<CellTypeDomain>>();
            }
            set
            {
                SetProperty(value);
                SelectedCellType = value.FirstOrDefault();
            }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
                if (value?.AnalysisDomain != null)
                {
                    SelectedMixingCycle = value.AnalysisDomain.MixingCycle;
                    SelectedAspirationCycle = value.AspirationCycles;
                }
            }
        }

        public eCellDeclusterSetting SelectedDecluster
        {
            get { return GetProperty<eCellDeclusterSetting>(); }
            set { SetProperty(value); }
        }

        public List<AnalysisDomain> AvailableAnalyses
        {
            get { return GetProperty<List<AnalysisDomain>>(); }
            private set { SetProperty(value); }
        }

        public string AcceptButtonIcon
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool IsDiagnosticAvailable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

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

        public bool IsEditControlEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Commands

        public override bool CanAccept()
        {
            return SelectedCellType != null;
        }

        protected override void OnAccept()
        {
            if (IsDiagnosticAvailable) SelectedCellType.DeclusterDegree = SelectedDecluster;

            base.OnAccept();
        }

        #endregion

        private void LockStatusChanged(LockResult res)
        {
            if (res == LockResult.Locked)
            {
                Close(false);
            }
        }
    }
}