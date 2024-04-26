using ScoutUtilities.Common;
using ScoutViewModels.ViewModels;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScoutUI.Views.ScoutUIMain
{
    public partial class TitleBar : UserControl
    {
        private bool _isLongPress;
        private bool _mouseIsDown;
        private Stopwatch _stopwatch;

        public TitleBar()
        {
            InitializeComponent();
            _isLongPress = false;
            _mouseIsDown = false;
            _stopwatch = new Stopwatch();
        }

        private void btnUserProfile_TouchDown(object sender, TouchEventArgs e)
        {
            ActionDown();
        }
        private void btnUserProfile_TouchUp(object sender, TouchEventArgs e)
        {
            ActionUp();
        }
        private void btnUserProfile_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ActionDown();
        }
        private void btnUserProfile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            ActionUp();
        }

        private void ActionDown()
        {
            _isLongPress = false;
            _mouseIsDown = true;
            _stopwatch.Restart();
        }

        private void ActionUp()
        {
            _stopwatch.Stop();
            _mouseIsDown = false;
        }

        private void btnUserProfile_Click(object sender, RoutedEventArgs e)
        {
            if (_mouseIsDown && _stopwatch.ElapsedMilliseconds >= 2000 && !_isLongPress)
            {
                _isLongPress = true;
                ActionUp();
            }

            // This delay is required because the MouseUp/TouchUp event fires immediatly AFTER this event and
            // this is the only event that will allow the new dialog windows to open AND not require the user
            // click 11 times to activate the controls within.
            Task.Delay(100).ContinueWith((t) =>
            {
                if (!_mouseIsDown)
                    DispatcherHelper.ApplicationExecute(() => DoClickAction());
            });
        }

        private void DoClickAction()
        {
            if (DataContext is TitleBarViewModel vm)
            {
                if (_isLongPress)
                {
                    if (vm.ServiceUserLoginCommand?.CanExecute(null) == true)
                        vm.ServiceUserLoginCommand.Execute(null);
                }
                else
                {
                    if (vm.UserProfileCommand?.CanExecute(null) == true)
                        vm.UserProfileCommand.Execute(null);
                }
            }
        }
    }
}
