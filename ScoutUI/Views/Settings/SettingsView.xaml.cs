using System.Windows.Controls;
using ScoutViewModels.ViewModels;
using ScoutUI.Views.Storage.UserControls;

namespace ScoutUI.Views.Admin.UserControls
{
    public partial class SettingsView
    {
        public SettingsView()
        {
            InitializeComponent();
        }
        public void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbiStorage != null && tbiStorage.IsSelected)
            {
                if (e.Source.GetType() == typeof(TabControl))
                {
                    var svm = (SettingsViewModel)tbiStorage.DataContext;
                    svm.StorageTabViewModel.InitUserList();
                    svm.SelectedTabItem = ScoutUtilities.Enums.SettingsTab.Storage;
                }
            }

            if(tbiSecurity != null && tbiSecurity.IsSelected)
            {
                if (e.Source.GetType() == typeof(TabControl))
                {
                    var svm = (SettingsViewModel)tbiSecurity.DataContext;
                    svm.SelectedTabItem = ScoutUtilities.Enums.SettingsTab.Security;
                }
            }
        }
    }
}