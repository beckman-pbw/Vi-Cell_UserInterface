using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScoutViewModels.Common;

namespace ScoutUI.Views.Dialogs
{
    public class Dialog : Window
    {
        protected BaseDialogViewModel Context;
        private bool _isClosing;
        private bool _fadeMainWindow;
        private bool _mainWindowAlreadyFaded;

        private Point _parentWindowPosition;
        private Point _parentWindowDimensions;

        public Dialog() { }

        public Dialog(BaseDialogViewModel context)
        {
            Context = context;
            Context.RequestClose += OnRequestClose;

            MinHeight = 175;
            MinWidth = 250;

            _mainWindowAlreadyFaded = false;
            _fadeMainWindow = context.FadeBackground;
            _parentWindowPosition = context.ParentWindowPosition;
            _parentWindowDimensions = context.ParentWindowDimensions;

            if (context.SizeToContent)
            {
                SizeToContent = SizeToContent.WidthAndHeight;
            }

            DataContext = Context;

            Loaded += OnDialogLoaded;

            PreviewMouseDown += OnIdleStateAction;
            MouseDown += OnIdleStateAction;
            MouseEnter += OnIdleStateAction;
            MouseLeftButtonDown += OnIdleStateAction;
            PreviewKeyDown += OnIdleStateAction;
            TouchDown += OnIdleStateAction;
            TouchEnter += OnIdleStateAction;
            KeyDown += OnDialogKeyDown;
        }

        protected virtual void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            // If the user hits ESC, try to close the dialog.
            if (DataContext is BaseDialogViewModel vm)
            {
                if (e.Key.Equals(Key.Escape))
                {
                    if (vm.CancelCommand?.CanExecute(null) == true)
                        vm.CancelCommand?.Execute(null);
                }
            }
        }

        protected virtual void OnIdleStateAction(object sender, RoutedEventArgs e)
        {
            IdleState.ValidateIdleState(e.RoutedEvent.Name);
        }

        protected virtual void OnDialogLoaded(object sender, RoutedEventArgs e)
        {
            var dialogLocation = ((BaseDialogViewModel) DataContext).DialogLocation;
            SetDialogLocation(dialogLocation);
            Focus();
        }

        /// <summary>
        /// This will only work when called from Window.Loaded event.
        /// </summary>
        /// <param name="dialogLocation"></param>
        public void SetDialogLocation(DialogLocation dialogLocation)
        {
            switch (dialogLocation)
            { 
                case DialogLocation.TopCenterApp:
                    Left = _parentWindowPosition.X + (_parentWindowDimensions.X - ActualWidth) / 2;
                    Top = 45;
                    break;
                case DialogLocation.UpAndLeftOfCenter:
                    Left  = (_parentWindowPosition.X + (_parentWindowDimensions.X - ActualWidth) / 2) - 200;
                    Top = _parentWindowPosition.Y + (_parentWindowDimensions.Y - ActualHeight) / 2 - 30;
                    break;
                case DialogLocation.UpAndRightOfCenter:
                    Left = (_parentWindowPosition.X + (_parentWindowDimensions.X - ActualWidth) / 2) + 200;
                    Top = _parentWindowPosition.Y + (_parentWindowDimensions.Y - ActualHeight) / 2 - 30;
                    break;
                case DialogLocation.CenterScreen:
                    Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
                    Top = (SystemParameters.PrimaryScreenHeight - ActualHeight) / 2;
                    break;
                case DialogLocation.TopCenterScreen:
                    Left = (SystemParameters.PrimaryScreenWidth - ActualWidth) / 2;
                    Top = 45;
                    break;
                case DialogLocation.MessageHubLocation:
                    Left = _parentWindowPosition.X + 250;
                    Top = _parentWindowPosition.Y + 58;
                    break;
                case DialogLocation.FullScreen:
                    Left = 0;
                    Top = 0;
                    Width = SystemParameters.PrimaryScreenWidth;
                    Height = SystemParameters.PrimaryScreenHeight;
                    WindowState = WindowState.Maximized;
                    break;
                
                default: // DialogLocation.CenterApp
                    Left = _parentWindowPosition.X + (_parentWindowDimensions.X - ActualWidth) / 2;
                    Top = _parentWindowPosition.Y + (_parentWindowDimensions.Y - ActualHeight) / 2;
                    break;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            MessageBus.Default.Publish(new Notification<bool>(MessageToken.MainVm, MessageToken.AdornerVisible, b => { _mainWindowAlreadyFaded = b; }));

            if (_fadeMainWindow && !_mainWindowAlreadyFaded)
            {
                MessageBus.Default.Publish(new Notification<bool>(true, MessageToken.MainVm, MessageToken.AdornerVisible));
            }

            ResizeMode = ResizeMode.NoResize; // maybe this will change later
            Focus();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _isClosing = true;

            if (Context.Close(null))
            {
                try
                {
                    DialogResult = Context.DialogResult;
                }
                catch (InvalidOperationException)
                {
                    // Dialog was probably opened by calling Show()
                }

                base.OnClosing(e);
            }
            else
            {
                e.Cancel = true; // context didn't close -- cancel the dialog close
            }

            _isClosing = false;
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_fadeMainWindow && !_mainWindowAlreadyFaded)
            {
                MessageBus.Default.Publish(new Notification<bool>(false, MessageToken.MainVm, MessageToken.AdornerVisible));
            }

            PreviewMouseDown -= OnIdleStateAction;
            MouseDown -= OnIdleStateAction;
            MouseEnter -= OnIdleStateAction;
            MouseLeftButtonDown -= OnIdleStateAction;
            PreviewKeyDown -= OnIdleStateAction;
            TouchDown -= OnIdleStateAction;
            TouchEnter -= OnIdleStateAction;

            base.OnClosed(e);

            Context.RequestClose -= OnRequestClose;
        }

        private void OnRequestClose(object sender, EventArgs args)
        {
            if(!_isClosing)
            {
                Dispatcher?.Invoke(Close);
            }
        }

        public void Button_TouchDown(object sender, TouchEventArgs e)
        {
            // There's an issue with some dialog windows having to be touched 11 times before they
            // activate. This is a workaround for the issue.
            // https://github.com/dotnet/wpf/issues/194
            var command = ((Button) sender)?.Command;
            var param = ((Button) sender)?.CommandParameter;
            var vm = (BaseDialogViewModel) DataContext;
            if (vm == null || command == null)
            {
                e.Handled = true;
                return;
            }

            if (command.CanExecute(param))
            {
                command.Execute(param);
            }

            e.Handled = true; // Need to mark handled before closing the dialog (PC3549-3836) to prevent the window below to trigger
        }

        public void TextBox_TouchDown(object sender, TouchEventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox == null) return;
            textBox.Focus();
            textBox.CaretIndex = Math.Max(0, textBox.Text?.Length ?? 0);
            textBox.ForceCursor = true;
            e.Handled = true;
        }

        public void PasswordBox_TouchDown(object sender, TouchEventArgs e)
        {
            var passwordBox = (PasswordBox)sender;
            if (passwordBox == null) return;
            passwordBox.Focus();
            passwordBox.ForceCursor = true;
            e.Handled = true;
        }
    }
}
