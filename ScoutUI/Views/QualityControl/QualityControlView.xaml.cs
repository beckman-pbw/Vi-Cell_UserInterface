using System.Windows.Controls;
using System.Windows.Input;

namespace ScoutUI.Views.QualityControl.UserControls
{
    /// <summary>
    /// Interaction logic for QualityControlView.xaml
    /// </summary>
    public partial class QualityControlView
    {
        public QualityControlView()
        {
            InitializeComponent();
        }

        private void DgrGridQueue_OnTouchLeave(object sender, TouchEventArgs e)
        {
            var dtGrid = sender as DataGrid;
            if (dtGrid != null)
                dtGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
        }

        private void DgrGridQueue_OnTouchEnter(object sender, TouchEventArgs e)
        {
            var dtGrid = sender as DataGrid;
            if (dtGrid != null)
                dtGrid.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
        }
    }
}