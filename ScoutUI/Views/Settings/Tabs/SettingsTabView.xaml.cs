using ScoutViewModels.ViewModels.Tabs;

namespace ScoutUI.Views.Settings
{
    /// <summary>
    /// Interaction logic for SettingView.xaml
    /// </summary>
    public partial class SettingsTabView
    {
        public SettingsTabView()
        {
            InitializeComponent();
            Loaded += SettingsTabView_Loaded;
        }

        private void SettingsTabView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is SettingsTabViewModel vm)
            {
                vm.OnViewLoaded();
            }
        }
    }
}