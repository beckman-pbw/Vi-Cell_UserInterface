using System.Windows;
using System.Windows.Input;
using ScoutViewModels.ViewModels.Reports;

namespace ScoutUI.Views.Admin.Reports
{
    /// <summary>
    /// Interaction logic for ResultsQualityControlUserControlView.xaml
    /// </summary>
    public partial class ResultsQualityControlUserControlView
    {
        public ResultsQualityControlUserControlView()
        {
            InitializeComponent();
        }

        private void GdOpenSample_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Escape) && gdOpenSample.Visibility == Visibility.Visible)
            {
                if (DataContext is ResultsQualityControlViewModel vm)
                {
                    vm.PerformCloseSelectSample();
                }
            }
        }
    }
}