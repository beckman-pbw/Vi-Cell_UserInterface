using log4net;
using ScoutUI.Views.Dialogs;
using ScoutUtilities.Common;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels;
using System.Windows;
using System.Windows.Input;
using ScoutModels.Common;
using ScoutModels.Interfaces;

namespace ScoutUI.Views.ScoutUIMain
{
    public partial class MainWindow
    {
        private DialogEventManager _dialogEventManager;
        private ReportEventManager _reportEventManager;
        private readonly IScoutViewModelFactory _viewModelFactory;
        private readonly IApplicationStateService _applicationStateService;
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow(MainWindowViewModel mainWindowViewModel, IScoutViewModelFactory viewModelFactory, IApplicationStateService applicationStateService)
        {
            InitializeComponent();
            DataContext = mainWindowViewModel;
            Loaded += ScoutUIMasterView_Loaded;

            _viewModelFactory = viewModelFactory;
            _applicationStateService = applicationStateService;

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Height = SystemParameters.PrimaryScreenHeight;
            Width = SystemParameters.PrimaryScreenWidth;

            if (!UISettings.IsFromHardware && UISettings.UseWindowedMode)
            {
                MainWindowContentGrid.Margin = new Thickness(0, 0, 10, 37);
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = WindowState.Normal;
                ResizeMode = ResizeMode.CanResize;
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
                Height = ApplicationConstants.WindowHeight;
                Width = ApplicationConstants.WindowWidth;
            }

            MessageBus.Default.Subscribe<Notification>(HandleNotificationMessages);
        }

        private void HandleNotificationMessages(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token) || string.IsNullOrEmpty(msg?.Message))
                return;

            if (msg.Token.Equals(MessageToken.MinimizeApplicationWindow))
            {
                WindowState = WindowState.Minimized;
            }
        }

        private void ScoutUIMasterView_Loaded(object sender, RoutedEventArgs e)
        {
            _dialogEventManager = new DialogEventManager(this, _viewModelFactory);
            _reportEventManager = new ReportEventManager(this);
        }

        private void ScrollViewer_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {
            e.Handled = true;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dialogEventManager.Dispose();
            _reportEventManager.Dispose();

            var masterViewModel = DataContext as MainWindowViewModel;
            _applicationStateService.PublishStateChange(ApplicationStateEnum.Shutdown);

            e.Cancel = true; // Ignoring event as application closing on shutdown
        }

        private void ShadowMouseDown(object sender, MouseButtonEventArgs e)
        {
            if(DataContext is MainWindowViewModel vm)
            {
                vm.IsNavigationMenuOpen = false;
            }
        }
    }
}
