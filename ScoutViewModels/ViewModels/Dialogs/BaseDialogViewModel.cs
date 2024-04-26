using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using System;
using System.Windows;
using System.Windows.Media;
using Point = System.Windows.Point;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class BaseDialogViewModel : BaseViewModel
    {
        #region Properties & Fields

        private bool _isClosed;
        public bool? DialogResult { get; protected set; }
        public DialogLocation DialogLocation { get; protected set; }
        public EventHandler RequestClose { get; set; }

        public bool FadeBackground { get; set; }
        public bool SizeToContent { get; set; }
        
        public Point ParentWindowPosition { get; set; }
        public Point ParentWindowDimensions { get; set; }

        public virtual bool ShowDialogTitleBar
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public virtual string DialogTitle
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public DrawingBrush DialogIconDrawBrush
        {
            get { return GetProperty<DrawingBrush>(); }
            set { SetProperty(value); }
        }

        public string DialogIconPath
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        public bool DialogIconIsSquare
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public int CloseButtonWidth
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public int CloseButtonHeight
        {
            get { return GetProperty<int>(); }
            set { SetProperty(value); }
        }

        public string ExtraTitleBarText
        {
            get { return GetProperty<string>(); }
            set { SetProperty(value); }
        }

        #endregion

        protected BaseDialogViewModel(BaseDialogEventArgs args, Window parentWindow)
        {
            ShowDialogTitleBar = true;
            DialogTitle = LanguageResourceHelper.Get("LID_Title_ViCellBlu");
            
            FadeBackground = args.FadeBackground;
            SizeToContent = args.SizeToContent;
            DialogIconIsSquare = args.DialogIconIsSquare;
            DialogIconPath = args.DialogIconPath ?? string.Empty;
            CloseButtonHeight = args.CloseButtonSize;
            CloseButtonWidth = args.CloseButtonSize;
            DialogLocation = args.DialogLocation;

            SetParentWindowPositions(parentWindow);
        }

        public void SetParentWindowPositions(Window parentWindow)
        {
            if (parentWindow == null)
            {
                ParentWindowPosition = new Point(0, 0);
                ParentWindowDimensions = new Point(ApplicationConstants.WindowWidth, ApplicationConstants.WindowHeight);
                return;
            }

            ParentWindowPosition = new Point(0, 0);
            ParentWindowDimensions = new Point(parentWindow.ActualWidth, parentWindow.ActualHeight);

            try
            {
                var field = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var left = (double)(field?.GetValue(parentWindow) ?? 0);

                field = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var top = (double)(field?.GetValue(parentWindow) ?? 0);

                ParentWindowPosition = new Point(left, top);
            }
            catch (Exception)
            {
                // don't worry about it
            }
        }

        public virtual bool Close(bool? result)
        {
            if(!_isClosed && CanClose(result))
            {
                _isClosed = true;
                DialogResult = result;
                OnClosed(result);
                RequestClose?.Invoke(this, EventArgs.Empty);
                Dispose();
            }
            return _isClosed;
        }

        public virtual void OnClosed(bool? result) { }

        public virtual bool CanClose(bool? dialogResult)
        {
            var canAccept = dialogResult == true && CanAccept();
            var canDecline = dialogResult == false && CanDecline();
            var canCancel = dialogResult == null && CanCancel();
            return canAccept || canDecline || canCancel;
        }

        #region Standard Dialog Commands

        private RelayCommand _acceptCommand;
        public virtual RelayCommand AcceptCommand
        {
            get
            {
                if (_acceptCommand == null) _acceptCommand = new RelayCommand(OnAccept, CanAccept);
                return _acceptCommand;
            }
        }
        protected virtual void OnAccept() { Close(true); }
        public virtual bool CanAccept() { return true; }

        private RelayCommand _declineCommand;
        public virtual RelayCommand DeclineCommand
        {
            get
            {
                if (_declineCommand == null) _declineCommand = new RelayCommand(OnDecline, CanDecline);
                return _declineCommand;
            }
        }
        protected virtual void OnDecline() { Close(false); }
        public virtual bool CanDecline() { return true; }

        private RelayCommand _cancelCommand;
        public virtual RelayCommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null) _cancelCommand = new RelayCommand(OnCancel, CanCancel);
                return _cancelCommand;
            }
        }
        protected virtual void OnCancel() { Close(null); }
        public virtual bool CanCancel() { return true; }

        #endregion
    }
}
