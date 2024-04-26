using ScoutViewModels.ViewModels.Service.ConcentrationSlope;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ScoutUI.Views.Service.ConcentrationSlope
{
    /// <summary>
    /// Interaction logic for AcupDataPanel.xaml
    /// </summary>
    public partial class AcupDataPanel : UserControl
    {
        public AcupDataPanel()
        {
            InitializeComponent();
            Loaded += AcupDataPanelView_Loaded;
        }

        private void AcupDataPanelView_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is AcupDataPanelViewModel vm)
            {
                vm.OnViewLoaded();
            }
        }
    }
}
