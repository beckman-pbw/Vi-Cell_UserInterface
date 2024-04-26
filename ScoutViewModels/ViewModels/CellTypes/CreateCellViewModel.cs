using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutModels.Admin;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using ScoutViewModels.ViewModels.Common;
using System;
using System.Linq;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;
using ScoutModels;
using ScoutModels.Interfaces;
using System.Collections.Generic;
using ScoutServices.Enums;

namespace ScoutViewModels.ViewModels.CellTypes
{
    public class CreateCellViewModel : BaseCellTypeViewModel 
    {
        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly ILockManager _lockManager;

        #region Construtor

        public CreateCellViewModel(UserDomain currentUser, IScoutViewModelFactory viewModelFactory,
             IInstrumentStatusService instrumentStatusService, ICellTypeManager cellTypeManager, ILockManager lockManager): base(currentUser, instrumentStatusService, cellTypeManager)
        {
            _viewModelFactory = viewModelFactory;
            _lockManager = lockManager;
            _lockStateSubscriber = _lockManager.SubscribeStateChanges().Subscribe(LockStatusChanged);
        }

        protected override void DisposeUnmanaged()
        {
            OpenCellVM?.Dispose();
            ReviewViewModel?.Dispose();
            _lockStateSubscriber?.Dispose();
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private bool _isInitialized;
        private IDisposable _lockStateSubscriber;

        public OpenCellTypeViewModel OpenCellVM { get; set; }
        public ReviewViewModel ReviewViewModel { get; set; }

        public bool IsEditEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCopyEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsCopyActive
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsEditActive
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                if (value)
                   IsCopyEnable = false;
            }
        }

        public CellTypeDomain SelectedCellFromList
        {
            get { return GetProperty<CellTypeDomain>(); }
            set { SetProperty(value); }
        }

        private CellTypeDomain _selectedCellType;
        public CellTypeDomain SelectedCellType
        {
            get { return _selectedCellType ?? (_selectedCellType = new CellTypeDomain()); }
            set
            {
                if (_selectedCellType == value) return;

                _selectedCellType = value;
                NotifyPropertyChanged(nameof(SelectedCellType));
                if (!_isInitialized)
                {
                    TempName = SelectedCellType.CellTypeName;
                    _isInitialized = true;
                }

                if (value == null) return;

                SelectedCellFromList = (CellTypeDomain)_selectedCellType.Clone();
                SetSelectedCellValues(SelectedCellFromList);
                IsCopyEnable = true;

                if (_selectedCellType.CellTypeName.Contains(ApplicationConstants.SelectedSampleCellIndication))
                {
                    ReviewViewModel.RetainOriginalResult();
                }

                IsEditEnable = SelectedCellFromList.IsUserDefineCellType &&
                               !SelectedCellFromList.IsQualityControlCellType &&
                               !_lockManager.IsLocked();
            }
        }

        private string _tempName;
        public string TempName
        {
            get { return _tempName; }
            set
            {
                _tempName = SelectedCellType.CellTypeName;
                if (_tempName == value) return;

                _tempName = value;
                NotifyPropertyChanged(nameof(PropertyChanged));
                SelectedCellType.TempCellName = _tempName;

                if (IsValidCellType())
                {
                    IsCopyEnable = true;
                }
                else
                {
                    IsCopyEnable = IsEditEnable = false;
                }
            }
        }

        #endregion

        #region Event Handlers

        protected override void OnSystemStatusChanged()
        {
            RaiseCommandsCanExecuteChanged();
        }

        private void LockStatusChanged(LockResult res)
        {
            IsEditEnable = SelectedCellFromList != null &&
                SelectedCellFromList.IsUserDefineCellType &&
                !SelectedCellFromList.IsQualityControlCellType &&
                (res != LockResult.Locked);
        }

        #endregion

        #region Command

        #region Edit Cell Command

        private RelayCommand _editCommand;
        public RelayCommand EditCommand => _editCommand ?? (_editCommand = new RelayCommand(PerformEditCommand));

        /// <summary>
        /// Makes all the cell type fields editable, makes the copy button disabled, and makes save/cancel buttons visible
        /// </summary>
        private void PerformEditCommand()
        {
            IsEditActive = true;
        }

        #endregion

        #region Copy Cell Command

        private RelayCommand _copyCellTypeCommand;
        public RelayCommand CopyCellTypeCommand => _copyCellTypeCommand ?? (_copyCellTypeCommand = new RelayCommand(PerformCopyCellType));

        /// <summary>
        /// Creates a copy of the given cell type without modifying it other than the cell type name.
        /// </summary>
        private void PerformCopyCellType()
        {
            IsCopyActive = true;
            var tempCellTypeName = string.Format(ScoutLanguageResources.LanguageResourceHelper.Get("LID_RadioButtonTab_CopyTempCell"), SelectedCellFromList.TempCellName);

            var args = new SaveCellTypeEventArgs(tempCellTypeName, false);
            if (DialogEventBus.SaveCellTypeDialog(this, args) == true)
            {
                SelectedCellFromList.TempCellName = args.CellTypeName;
                SaveCellType();
            }
			
            OnReturnToView();
		}

        #endregion

        #region Modify Cell Command

        private RelayCommand _editSaveCellTypeCommand;
        public RelayCommand EditSaveCellTypeCommand => _editSaveCellTypeCommand ?? (_editSaveCellTypeCommand = new RelayCommand(PerformEditSaveCellType));

        private void PerformEditSaveCellType()
        {
            if (!_cellTypeManager.SaveCellTypeValidation(SelectedCellFromList, true)) return;

            // prompt for valid credentials, if applicable
            if (SecurityCheck() != LoginResult.CurrentUserLoginSuccess) return;

            // If in edit mode append the localized date/time (???) to the CellType name to be passed to the Backend.
            if (IsEditActive)
            {
	            SelectedCellFromList.TempCellName += " (" + Misc.DateFormatConverter(DateTime.Now, "LongDate") + ")";
            }
            else
            {
	            SelectedCellFromList.CellTypeName = SelectedCellFromList.TempCellName;
	            SelectedCellFromList.TempCellName = "";
            }

			if (AddUserDefineCellType(ScoutModels.LoggedInUser.CurrentUserId, "", SelectedCellFromList))
				IsEditActive = false;

            OnReturnToView();
        }

        #endregion

        #region Cancel Command

        private RelayCommand _cancelCommand;
        public override RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(PerformCancelCommand));

        /// <summary>
        /// Cancels the cell type edit
        /// </summary>
        private void PerformCancelCommand()
        {
            OnReturnToView();
            CloseWindow(null);
        }

        #endregion

        #region Close Create Cell Dialog Command

        private RelayCommand _closeCreateCellViewCommand;
        public RelayCommand CloseCreateCellViewCommand => _closeCreateCellViewCommand ?? (_closeCreateCellViewCommand = new RelayCommand(PerformCloseCreateCell));

        /// <summary>
        /// Closes the reanalyze content and switches back to the standard cell type page
        /// </summary>
        private void PerformCloseCreateCell()
        {
            try
            {
                MainWindowViewModel.Instance.SetContent(_viewModelFactory.CreateCellTypeViewModel(CurrentUser));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    ScoutLanguageResources.LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONCLOSECREATECELLVIEW"));
            }
        }

        #endregion

        #endregion

        #region Private Methods

        private bool IsValidCellType()
        {
            return SelectedCellType.CellTypeName != null && SelectedCellType.CellTypeName.Equals(SelectedCellType.TempCellName);
        }

        private void SetValueForCell()
        {
            if (SelectedCellType.TempCellName.Contains(ApplicationConstants.SelectedSampleCellIndication))
            {
                var cellName = SelectedCellType.TempCellName.Remove(SelectedCellType.TempCellName.Length - 1);
                SelectedCellFromList.TempCellName = cellName;
                SelectedCellFromList.CellTypeName = cellName;
            }
            else
            {
                SelectedCellType.TempCellName = SelectedCellFromList.TempCellName;
                SelectedCellType.CellTypeName = SelectedCellType.TempCellName;
                SelectedCellFromList.CellTypeName = SelectedCellType.CellTypeName;
            }
            SelectedCellFromList.IsUserDefineCellType = SelectedCellType.IsUserDefineCellType;
        }

        private void OnReturnToView()
        {
            IsEditActive = IsCopyActive = false;
            if (LoggedInUser.IsConsoleUserLoggedIn())
            {
                AvailableCellTypes = LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection();
            }
            else
            {
                AvailableCellTypes = new System.Collections.ObjectModel.ObservableCollection<CellTypeDomain>();
            }
            if (AvailableCellTypes.Count <= 0) return;

            SelectedCellType = !string.IsNullOrEmpty(SelectedCellFromList.CellTypeName)
                ? AvailableCellTypes.FirstOrDefault(c => c.CellTypeName.Equals(SelectedCellFromList.CellTypeName))
                : AvailableCellTypes[0];
        }

        private void SaveCellType()
        {
            try
            {
                if (SelectedCellFromList == null) return;

                SetValueForCell();
                if (!_cellTypeManager.SaveCellTypeValidation(SelectedCellFromList, true))
	                return;

                var username = ScoutModels.LoggedInUser.CurrentUserId;
                if (AddUserDefineCellType(username, "", SelectedCellFromList))
                {
                    AvailableCellTypes = LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection();
                    SelectedCellType = AvailableCellTypes.First(c => c.CellTypeName == SelectedCellFromList.CellTypeName);
                    if (ReviewViewModel?.SelectedSampleRecord?.SelectedResultSummary?.CellTypeDomain != null)
                    {
                        var cell = AvailableCellTypes.FirstOrDefault(x =>
                            x.CellTypeName.Equals(ReviewViewModel.SelectedSampleRecord.SelectedResultSummary
                            .CellTypeDomain.TempCellName));
                        if (cell != null)
                        {
                            cell.CellTypeName = cell.CellTypeName + ApplicationConstants.SelectedSampleCellIndication;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    ScoutLanguageResources.LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONADDCELLTYPE"));
            }
        }

        private void RaiseCommandsCanExecuteChanged()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                EditCommand.RaiseCanExecuteChanged();
                CopyCellTypeCommand.RaiseCanExecuteChanged();
                EditSaveCellTypeCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                CloseCreateCellViewCommand.RaiseCanExecuteChanged();
            });
        }

        #endregion

        #region Public Methods

        public void SetSelectedCellType(CellTypeDomain selectedCellType)
        {
            SelectedCellType = selectedCellType;
            SelectedCellFromList = selectedCellType;
            SetSelectedCellValues(SelectedCellFromList);
        }

        public Tuple<CellTypeDomain, CellTypeDomain> GetSelectedCell(bool isValue)
        {
            SetCellTypeValue(SelectedCellFromList);
            return new Tuple<CellTypeDomain, CellTypeDomain>(SelectedCellFromList, SelectedCellType);
        }

        public void UpdateSelectedSampleCell(object cellType)
        {
            var selectedCell = cellType as CellTypeDomain;
            if(LoggedInUser.IsConsoleUserLoggedIn())
            {
                AvailableCellTypes = LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection();
            }
            else
                AvailableCellTypes = new System.Collections.ObjectModel.ObservableCollection<CellTypeDomain>();

            if (selectedCell == null)
                return;
            var cell = AvailableCellTypes.First(x => x.CellTypeName.Equals(selectedCell.CellTypeName));
            if (cell != null)
            {
                cell.CellTypeName = cell.CellTypeName + ApplicationConstants.SelectedSampleCellIndication;
                SelectedCellType = cell;
            }
            else
            {
                SelectedCellType = AvailableCellTypes.First();
            }
        }

        #endregion
    }
}
