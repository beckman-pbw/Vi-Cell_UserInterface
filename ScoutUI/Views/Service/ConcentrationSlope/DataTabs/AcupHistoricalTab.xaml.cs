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
using log4net;

namespace ScoutUI.Views.Service.ConcentrationSlope.DataTabs
{
    /// <summary>
    /// Interaction logic for AcupHistoricalTab.xaml
    /// </summary>
    public partial class AcupHistoricalTab : UserControl
    {
	    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AcupHistoricalTab()
        {
            InitializeComponent();
        }

        private void EatMouseUpEventToPreventCrash(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
	        // see PC3549-5381 App crash when clicking on concentration slope history
	        Log.Info("EatMouseUpEventToPreventCrash");
	        e.Handled = true;
        }
    }
}
