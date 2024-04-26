using System.Collections.Generic;
using HawkeyeCoreAPI;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Helper;
using System.Collections.ObjectModel;
using System.Linq;
using HawkeyeCoreAPI.Facade;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SaveCellTypeViewModel : BaseDialogViewModel
    {
        // Saved to pass back new cell type name. Should have used a reference to this view model
        private SaveCellTypeEventArgs _args;

        /// <summary>
        /// Provide a way to change the cell type name.
        /// </summary>
        /// <param name="args">Used to pass back the cell type name.</param>
        /// <param name="parentWindow"></param>
        public SaveCellTypeViewModel(SaveCellTypeEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            _args = args;
            DialogTitle = LanguageResourceHelper.Get("LID_Label_CopyCellType");
            ShowDialogTitleBar = true;
            CellTypeName = args.CellTypeName;
            AvailableCellTypes = LoggedInUser.CurrentUser?.AssignedCellTypes ?? new List<CellTypeDomain>();
        }

        public List<CellTypeDomain> AvailableCellTypes { get; set; }

        public ObservableCollection<CellTypeDomain> AllCellTypesList
        {
            get
            {
                var item = GetProperty<ObservableCollection<CellTypeDomain>>();
                if (item != null) return item;
                if (LoggedInUser.CurrentUser?.AssignedCellTypes != null)
                    SetProperty(LoggedInUser.CurrentUser.AssignedCellTypes.ToObservableCollection());
                return GetProperty<ObservableCollection<CellTypeDomain>>();
            }
        }

        public string CellTypeName
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Check whether the new cell type name does not already exist.
        /// </summary>
        /// <returns>true if it does not exist, false otherwise.</returns>
        private bool ValidateCellTypeNameAvailable()
        {
            return null == AvailableCellTypes.FirstOrDefault(c => c.CellTypeName.Equals(CellTypeName));
        }

        public override bool CanAccept()
        {
            return !string.IsNullOrEmpty(CellTypeName);
        }

        protected override void OnAccept()
        {
            if (!ValidateCellTypeNameAvailable())
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_ChooseADifferent_Name"));
                return;
            }

            // The caller should have retained a reference to this view model. Passing back using args is not appropriate.
            _args.CellTypeName = CellTypeName;
            if (!IsSecurityTurnedOn)
            {
                Close(true);
                return;
            }

            var args = new LoginEventArgs(LoggedInUser.CurrentUserId, LoggedInUser.CurrentUserId, LoginState.ValidateCurrentUserOnly, DialogLocation.CenterApp);
            var result = DialogEventBus.Login(this, args);
            if (result == LoginResult.CurrentUserLoginSuccess)
            {
                Close(true);
            }
        }
    }
}