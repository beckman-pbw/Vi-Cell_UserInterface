using System.Windows;
using log4net;

namespace ScoutUI.Views.Service
{
    public partial class ConcentrationView
    {
	    private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ConcentrationView()
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