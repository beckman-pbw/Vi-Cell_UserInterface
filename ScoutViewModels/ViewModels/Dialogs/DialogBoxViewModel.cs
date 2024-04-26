using ScoutLanguageResources;
using ScoutUtilities.CustomEventArgs;
using System.Windows;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class DialogBoxViewModel : BaseDialogViewModel
    {
        #region Properties

        public string Message
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool ShowAcceptButton
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string AcceptButtonText
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool ShowDeclineButton
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public string DeclineButtonText
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DialogButtons DialogButtons { get; set; }
        public MessageBoxImage Icon { get; set; }

        public bool ShowMessageIcon
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool ShowTextEntry
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string TextEntry
        {
            get { return GetProperty<string>(); }
            set 
            { 
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowMathFormula
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public string MathFormula
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool ShowCheckBox
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        public bool DeleteAllACupData
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                AcceptCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        public DialogBoxViewModel(DialogBoxEventArgs args, Window parentWindow) : base(args, parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = args.Title ?? LanguageResourceHelper.Get("LID_Title_ViCellBlu");
            Message = args.Message;
            Icon = args.Icon;
            ShowMessageIcon = args.Icon != MessageBoxImage.None;
            ShowTextEntry = args.ShowTextEntry;
            TextEntry = args.TextEntered ?? string.Empty;
            
            ShowMathFormula = !string.IsNullOrEmpty(args.MathFormula);
            MathFormula = args.MathFormula ?? string.Empty;
            ShowCheckBox = args.ShowCheckBox;
            DeleteAllACupData = args.DeleteAllACupData;

            SetButtons(args);
        }

        public override bool CanAccept()
        {
            if (ShowTextEntry)
            {
                return base.CanAccept() && !string.IsNullOrEmpty(TextEntry);
            }
            return base.CanAccept();
        }

        private void SetButtons(DialogBoxEventArgs args)
        {
            DialogButtons = args.DialogButtons;

            switch (args.DialogButtons)
            {
                case DialogButtons.Ok:
                    ShowAcceptButton = true;
                    ShowDeclineButton = false;
                    AcceptButtonText = string.IsNullOrEmpty(args.AcceptButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_OK") : args.AcceptButtonText;
                    DeclineButtonText = string.IsNullOrEmpty(args.DeclineButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_Cancel") : args.DeclineButtonText;
                    break;
                case DialogButtons.OkCancel:
                    ShowAcceptButton = true;
                    ShowDeclineButton = true;
                    AcceptButtonText = string.IsNullOrEmpty(args.AcceptButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_OK") : args.AcceptButtonText;
                    DeclineButtonText = string.IsNullOrEmpty(args.DeclineButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_Cancel") : args.DeclineButtonText;
                    break;
                case DialogButtons.YesNo:
                    ShowAcceptButton = true;
                    ShowDeclineButton = true;
                    AcceptButtonText = string.IsNullOrEmpty(args.AcceptButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_Yes") : args.AcceptButtonText;
                    DeclineButtonText = string.IsNullOrEmpty(args.DeclineButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_No") : args.DeclineButtonText;
                    break;
                case DialogButtons.Continue:
                    ShowAcceptButton = true;
                    ShowDeclineButton = false;
                    AcceptButtonText = string.IsNullOrEmpty(args.AcceptButtonText) ? LanguageResourceHelper.Get("LID_Button_Continue") : args.AcceptButtonText;
                    DeclineButtonText = string.IsNullOrEmpty(args.DeclineButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_Cancel") : args.DeclineButtonText;
                    break;
                case DialogButtons.ContinueWorking:
                    ShowAcceptButton = true;
                    ShowDeclineButton = false;
                    AcceptButtonText = string.IsNullOrEmpty(args.AcceptButtonText) ? LanguageResourceHelper.Get("LID_Label_ContinueWorking") : args.AcceptButtonText;
                    DeclineButtonText = string.IsNullOrEmpty(args.DeclineButtonText) ? LanguageResourceHelper.Get("LID_ButtonContent_Cancel") : args.DeclineButtonText;
                    break;
            }
        }
    }
}
