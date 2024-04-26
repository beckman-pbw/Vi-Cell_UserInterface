using ScoutViewModels.ViewModels.Home;
using System.Windows.Controls;

namespace ScoutUI.Views._1___Home
{
    public partial class HomeView : UserControl
    {
        public HomeView()
        {
            InitializeComponent();

            Loaded += HomeView_Loaded;
        }

        private void HomeView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is HomeViewModel vm)
            {
                vm.OnViewLoaded();
            }
        }
    }
}
