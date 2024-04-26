using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class AddSignatureViewModel : BaseDialogViewModel
    {
        public AddSignatureViewModel(AddSignatureEventArgs args, System.Windows.Window parentWindow) : base(args, parentWindow)
        {
            DialogTitle = LanguageResourceHelper.Get("LID_POPUPHeader_Signature");
            SignatureUserId = LoggedInUser.CurrentUserId;
            AvailableSignatureTypes = args.AvailableSignatures == null
                ? new ObservableCollection<ISignature>()
                : new ObservableCollection<ISignature>(args.AvailableSignatures);
        }

        #region Properties

        public string SignatureUserId
        {
            get { return GetProperty<string>(); }
            private set
            {
                SetProperty(value);
                AddSignatureCommand.RaiseCanExecuteChanged();
            }
        }

        public string SignaturePassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AddSignatureCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ISignature> AvailableSignatureTypes
        {
            get { return GetProperty<ObservableCollection<ISignature>>(); }
            set 
            { 
                SetProperty(value);
                SelectedSignatureType = value?.FirstOrDefault();
                AddSignatureCommand.RaiseCanExecuteChanged();
            }
        }

        public ISignature SelectedSignatureType
        {
            get { return GetProperty<ISignature>(); }
            private set { SetProperty(value); }
        }

        #endregion

        #region Commands

        private RelayCommand _addSignatureCommand;
        public RelayCommand AddSignatureCommand
        {
            get
            {
                if (_addSignatureCommand == null) _addSignatureCommand = new RelayCommand(AddSignature, CanAddSignature);
                return _addSignatureCommand;
            }
        }

        private bool CanAddSignature()
        {
            return !string.IsNullOrEmpty(SignatureUserId) && !string.IsNullOrEmpty(SignaturePassword);
        }

        private void AddSignature(object param)
        {
            if (string.IsNullOrEmpty(SignaturePassword))
            {
                DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_ERRMSGBOX_FieldBlank"));
                return;
            }

            var validateStatus = UserModel.ValidateMe(SignaturePassword);
            if (validateStatus.Equals(HawkeyeError.eSuccess))
            {
                if (param is ISignature sign) SelectedSignatureType = sign;
                Close(true);
            }
            else
            {
                ApiHawkeyeMsgHelper.ErrorValidateMe(validateStatus);
                SignaturePassword = string.Empty;
            }
        }

        #endregion
    }
}
