using ApiProxies.Generic;
using HawkeyeCoreAPI.Facade;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System;
using System.Linq;
using ScoutServices.Interfaces;
using ScoutViewModels.Interfaces;
using ScoutModels.Interfaces;
using ScoutModels;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.CellTypes
{
    public enum CellTypeViewSelection
    {
        ViewMode,
        EditMode,
        CopyMode
    }

    public class CellTypeViewModel : BaseCellTypeViewModel
    {
        #region Contructor

        public CellTypeViewModel(UserDomain currentUser, SystemStatusDomain currentSystemStatus, ILockManager lockManager, 
            IScoutViewModelFactory viewModelFactory, IInstrumentStatusService instrumentStatusService, ICellTypeManager cellTypeManager)
            : base(currentUser, instrumentStatusService, cellTypeManager)

        {
            _lockManager = lockManager;
            _viewModelFactory = viewModelFactory;
            _instrumentStatusService = instrumentStatusService;
            SetCellTypeList();
            _ctqcSubscriber = MessageBus.Default.Subscribe<Notification>(OnCellTypesQualityControlsUpdated);
        }

        protected override void DisposeUnmanaged()
        {
            MessageBus.Default.UnSubscribe(ref _ctqcSubscriber);
            base.DisposeUnmanaged();
        }
        #endregion

        #region Event Handlers

        private void OnCellTypesQualityControlsUpdated(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;

            if (msg.Token.Equals(MessageToken.CellTypesUpdated) || msg.Token.Equals(MessageToken.QualityControlsUpdated))
            {
                var name = SelectedCellType?.CellTypeName;
                SetCellTypeList(name);
            }
        }

        protected override void OnSystemStatusChanged()
        {
            RaiseAllCommands();
        }

        #endregion

        #region Properties & Fields

        private readonly ILockManager _lockManager;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IInstrumentStatusService _instrumentStatusService;
        private Subscription<Notification> _ctqcSubscriber;

        // Property for which radio button is currently selected
        public CellTypeViewSelection SelectedCellTypeView
        {
            get { return GetProperty<CellTypeViewSelection>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(CanEditName));
                NotifyPropertyChanged(nameof(AreFieldsEditable));
                RaiseAllCommands();
            }
        }

        // Controls all the UIElement fields IsEnabled values
        public bool CanEditName => SelectedCellTypeView != CellTypeViewSelection.EditMode;
                                         
        public bool AreFieldsEditable => SelectedCellTypeView == CellTypeViewSelection.EditMode ||
                                         SelectedCellTypeView == CellTypeViewSelection.CopyMode;

        public bool AreSaveDeleteCancelButtonsVisible
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public CellTypeDomain SelectedCellType
        {
            get { return GetProperty<CellTypeDomain>(); }
            set
            {
                SetProperty(value);
                SelectedCellTypeView = CellTypeViewSelection.ViewMode;
                if (value != null)
                {
                    SelectedCellTypeClone = (CellTypeDomain) value.Clone();
                }
            }
        }

        public CellTypeDomain SelectedCellTypeClone
        {
            get { return GetProperty<CellTypeDomain>(); }
            set
            {
                SetProperty(value);
                if (value != null)
                {
                    OnSelectionCellTypeChanged(value);
                }
            }
        }

        #endregion

        #region Commands

        #region Radio Button Commands

        #region View Cell Type Command

        private RelayCommand _viewCellTypeCommand;
        public RelayCommand ViewCellTypeCommand => _viewCellTypeCommand ?? (_viewCellTypeCommand = new RelayCommand(PerformViewCellType, CanPerformViewCellType));

        private bool CanPerformViewCellType()
        {
            return SelectedCellTypeView == CellTypeViewSelection.ViewMode;
        }

        private void PerformViewCellType()
        {
            try
            {
                SelectedCellTypeView = CellTypeViewSelection.ViewMode;
                if (AvailableCellTypes.Count > 0) SelectedCellType = AvailableCellTypes[0];
                AreSaveDeleteCancelButtonsVisible = false;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONSELECTION"));
            }
        }

        #endregion

        #region Edit Cell Type Command

        private RelayCommand _editCellTypeCommand;
        public RelayCommand EditCellTypeCommand => _editCellTypeCommand ?? (_editCellTypeCommand = new RelayCommand(PerformEditCellType, CanPerformEditCellType));

        private bool CanPerformEditCellType()
        {
            if (SelectedCellTypeClone == null)
            {
                return false;
            }

            return CurrentUser.RoleID != UserPermissionLevel.eService &&
	               SelectedCellTypeClone.IsUserDefineCellType &&
                   _cellTypeManager.InstrumentStateAllowsEdit &&
                   !SelectedCellTypeClone.IsQualityControlCellType &&
                   SelectedCellTypeView != CellTypeViewSelection.CopyMode &&
                   !_lockManager.IsLocked();
        }

        private void PerformEditCellType()
        {
            try
            {
                SelectedCellTypeView = CellTypeViewSelection.EditMode;
                SelectedCellTypeClone = (CellTypeDomain) SelectedCellType.Clone();
                AreSaveDeleteCancelButtonsVisible = true;
                SetSelectedCellValues(SelectedCellTypeClone);
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEDITCELLTYPE"));
            }
        }

        #endregion

        #region Copy Cell Type Command

        private RelayCommand _copyCellTypeCommand;
        public RelayCommand CopyCellTypeCommand => _copyCellTypeCommand ?? (_copyCellTypeCommand = new RelayCommand(PerformCopyCellType, CanPerformCopyCellType));

        private bool CanPerformCopyCellType()
        {
            return SelectedCellTypeView != CellTypeViewSelection.EditMode &&
                   CurrentUser.RoleID != UserPermissionLevel.eService;
        }

        private void PerformCopyCellType()
        {
            try
            {
                SelectedCellTypeView = CellTypeViewSelection.CopyMode;
                SelectedCellTypeClone = (CellTypeDomain) SelectedCellType.Clone();
                SelectedCellTypeClone.IsUserDefineCellType = true;
                SelectedCellTypeClone.IsQualityControlCellType = false;
                SelectedCellTypeClone.TempCellName = string.Format(LanguageResourceHelper.Get("LID_RadioButtonTab_CopyTempCell"), SelectedCellTypeClone.TempCellName);
                AreSaveDeleteCancelButtonsVisible = true;
                SetSelectedCellValues(SelectedCellTypeClone);
                NotifyPropertyChanged(nameof(SelectedCellTypeClone));
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONEDITCELLTYPE"));
            }
        }

        #endregion

        #endregion

        #region Normal Button Commands

        #region Adjustment Factor Help Command

        private RelayCommand _adjFactorInfoCommand;
        public RelayCommand AdjFactorInfoCommand => _adjFactorInfoCommand ?? (_adjFactorInfoCommand = new RelayCommand(PerformAdjFactorInfo));

        private void PerformAdjFactorInfo()
        {
            var adjFactor = LanguageResourceHelper.Get("LID_Label_AdjustmentFactor");
            var target = LanguageResourceHelper.Get("LID_Label_AdjustmentFactorInfoFormula_TargetConc");
            var measured = LanguageResourceHelper.Get("LID_Label_AdjustmentFactorInfoFormula_MeasuredConc");

			// Helpful links for Latex formula formatting:
			// https://quicklatex.com/
			// https://tex.stackexchange.com/questions/38868/big-parenthesis-in-an-equation
			// https://www.google.com/search?client=firefox-b-1-d&q=latex+math+spaces

			var mathFormula = adjFactor + 
                @" = \left( \frac" + "{" + target + " - " + measured + "}{" + measured + @"} \right) \:x\:100";

            var msg = LanguageResourceHelper.Get("LID_Label_AdjustmentFactorInfo") + 
                      Environment.NewLine + Environment.NewLine +
                      LanguageResourceHelper.Get("LID_Label_AdjustmentFactorInfoFormula");
            var dialogArgs = new DialogBoxEventArgs(msg, LanguageResourceHelper.Get("LID_Label_AdjustmentFactor"),
                mathFormula: mathFormula);
            DialogEventBus.DialogBox(this, dialogArgs);
        }

        #endregion

        #region Save Command

        private RelayCommand _saveCellTypeCommand;
        public RelayCommand SaveCellTypeCommand => _saveCellTypeCommand ?? (_saveCellTypeCommand = new RelayCommand(PerformSaveCommand, CanPerformSaveCommand));

        private bool CanPerformSaveCommand()
        {
            if (!_cellTypeManager.InstrumentStateAllowsEdit)
				return false;
            if (SelectedCellTypeView == CellTypeViewSelection.EditMode && _lockManager.IsLocked())
                return false;
			if (SelectedCellTypeView == CellTypeViewSelection.CopyMode ||
                SelectedCellTypeView == CellTypeViewSelection.EditMode)
                return true;

            if (AreSaveDeleteCancelButtonsVisible)
                return true;

            return false;
        }

        private void PerformSaveCommand()
        {
            if (SelectedCellTypeView == CellTypeViewSelection.EditMode && !ConfirmEditAction()) return;

            try
            {
                var username = ScoutModels.LoggedInUser.CurrentUserId;
                if (!_cellTypeManager.SaveCellTypeValidation(SelectedCellTypeClone, true)) return;

                // prompt for valid credentials, if applicable
                if (SecurityCheck() != LoginResult.CurrentUserLoginSuccess) return;

				// If in edit mode append the localized date/time (???) to the CellType name to be passed to the Backend.
				if (SelectedCellTypeView == CellTypeViewSelection.EditMode)
				{
					SelectedCellTypeClone.TempCellName += " (" + Misc.DateFormatConverter(DateTime.Now, "LongDate") + ")";
				}
				else
				{
					SelectedCellTypeClone.CellTypeName = SelectedCellTypeClone.TempCellName;
					SelectedCellTypeClone.TempCellName = "";
				}

				// Always copy the input CellType to a new object.
				// Always "add" the new/modified CellType.
				//   What needs to be cleared/reset to force the Backend to create a new CellType???
				// When editing a CellType, the CellType being edited must be marked as "retired" so that 
				// there are not multiple CellTypes with the same name.
				if (AddUserDefineCellType(username, "", SelectedCellTypeClone))
					OnRefreshCellView(SelectedCellTypeClone.CellTypeName);
				else
					OnRefreshCellView(SelectedCellType.CellTypeName);

			}
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONVALIDATIONOPENDIALOG"));
            }
        }

        #endregion

        #region Delete Command

        private RelayCommand _deleteCommand;
        public RelayCommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(PerformDeleteCommand, CanPerformDelete));

        private bool CanPerformDelete()
        {
            return (CurrentUser.RoleID != UserPermissionLevel.eService) && 
                _cellTypeManager.CanPerformDelete(SelectedCellTypeClone) &&
                !_lockManager.IsLocked();
        }

        private void PerformDeleteCommand()
        {
            if (!ConfirmDeleteAction()) return;

            // prompt for valid credentials, if applicable
            if (SecurityCheck() != LoginResult.CurrentUserLoginSuccess) return;

            try
            {
                RemoveCellType();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONVALIDATIONOPENDIALOG"));
            }
        }

        #endregion

        #region Cancel Command

        private RelayCommand _cancelCommand;
        public override RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(PerformCancelCommand, CanPerformCancelCommand));

        private bool CanPerformCancelCommand()
        {
            return true;
        }

        private void PerformCancelCommand()
        {
            SelectedCellTypeView = CellTypeViewSelection.ViewMode;
            OnRefreshCellView(SelectedCellTypeClone.CellTypeName);
        }

        #endregion

        #region Reanalyze Command
        
        private RelayCommand _reanalyzeCommand;
        public RelayCommand ReanalyzeCommand => _reanalyzeCommand ?? (_reanalyzeCommand = new RelayCommand(PerformReanalyze, CanPerformReanalyze));

        private bool CanPerformReanalyze()
        {
            return SelectedCellTypeView == CellTypeViewSelection.ViewMode &&
                   CurrentUser.RoleID != UserPermissionLevel.eService &&
                   _cellTypeManager.InstrumentStateAllowsEdit;
        }

        private void PerformReanalyze()
        {
            try
            {
                var createCellType = _viewModelFactory.CreateCreateCellTypeViewModel(CurrentUser);
                SelectedCellTypeClone = (CellTypeDomain) SelectedCellType.Clone();
                createCellType.SetSelectedCellType(SelectedCellTypeClone);

                MainWindowViewModel.Instance.SetContent(createCellType);

                createCellType.OpenCellVM = _viewModelFactory.CreateOpenCellTypeViewModel(CurrentUser);
                createCellType.OpenCellVM.GetSelectedCell = createCellType.GetSelectedCell;
                createCellType.OpenCellVM.ReviewViewModel.UpdateSelectedSampleCell = createCellType.UpdateSelectedSampleCell;
                createCellType.ReviewViewModel = createCellType.OpenCellVM.ReviewViewModel;
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex,
                    LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONCREATECELLTYPE"));
            }
        }

        #endregion

        #endregion

        #endregion

        #region Private Methods

        private bool ConfirmEditAction()
        {
            var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_ModifyConfirmation"), SelectedCellTypeClone.TempCellName);
            var result = DialogEventBus.DialogBoxYesNo(this, msg);
            return result == true;
        }

        private bool ConfirmDeleteAction()
        {
            var msg = string.Format(LanguageResourceHelper.Get("LID_MSGBOX_DeleteConfirmation"), SelectedCellTypeClone.TempCellName);
            var result = DialogEventBus.DialogBoxYesNo(this, msg);
            return result == true;
        }

        private void SetCellTypeList(string cellTypeToSelect = null)
        {
            if (LoggedInUser.CurrentUser == null)
            {
                AvailableCellTypes.Clear();
                return;
            }
            List<CellTypeDomain> cellTypes = new List<CellTypeDomain>();
            List<QualityControlDomain> _qcs = new List<QualityControlDomain>();
            List<CellTypeQualityControlGroupDomain> _ctqcgroup = new List<CellTypeQualityControlGroupDomain>();

            CellTypeFacade.Instance.GetAllowedCtQc(LoggedInUser.CurrentUserId, ref cellTypes, ref _qcs, ref _ctqcgroup);
            AvailableCellTypes = cellTypes.ToObservableCollection();

            if (AvailableCellTypes.Count == 0) return;

            if (!string.IsNullOrEmpty(cellTypeToSelect))
            {
                var cellType = AvailableCellTypes.FirstOrDefault(c => c.CellTypeName.Equals(cellTypeToSelect));
                SelectedCellType = cellType ?? AvailableCellTypes.FirstOrDefault();
            }
            else
            {
                SelectedCellType = AvailableCellTypes.FirstOrDefault();
            }

            SelectedCellTypeView = CellTypeViewSelection.ViewMode;
        }

        private void OnSelectionCellTypeChanged(CellTypeDomain selectedCell)
        {
            try
            {
                if (selectedCell == null) return;

                AreSaveDeleteCancelButtonsVisible = false;
                SelectedCellType.TempCellName = selectedCell.CellTypeName;
                RaiseAllCommands();
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_ERRORONSELECTION"));
            }
        }

        private void RemoveCellType()
        {
            if (SelectedCellTypeClone == null)
                return;
            var username = ScoutModels.LoggedInUser.CurrentUserId;
            if (_cellTypeManager.DeleteCellType(username, "", SelectedCellTypeClone, true))
                SetCellTypeList();

        }

        private void OnRefreshCellView(string cellName)
        {
            AreSaveDeleteCancelButtonsVisible = false;
            AreSaveDeleteCancelButtonsVisible = false;
            SetCellTypeList(cellName);
            RaiseAllCommands();
        }

        private void RaiseAllCommands()
        {
            DispatcherHelper.ApplicationExecute(() =>
            {
                ViewCellTypeCommand.RaiseCanExecuteChanged();
                EditCellTypeCommand.RaiseCanExecuteChanged();
                CopyCellTypeCommand.RaiseCanExecuteChanged();
                SaveCellTypeCommand.RaiseCanExecuteChanged();
                CancelCommand.RaiseCanExecuteChanged();
                ReanalyzeCommand.RaiseCanExecuteChanged();
                DeleteCommand.RaiseCanExecuteChanged();
            });
        }

        #endregion
    }
}
