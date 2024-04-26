using Winium.Cruciatus;
using Winium.Cruciatus.Core;
using System.Windows.Automation;
using ScoutUiTest.Map;

namespace ScoutUiTest
{
    public class ScoutXApp : Application
    {
        private MainWindow _mainWindow;
        #region Constructors and Destructors

        public ScoutXApp(string fullPath)
            : base(fullPath)
        {
        }

        #endregion

        #region Public Properties

        public MainWindow MainWindow
        {
            get
            {
                if (null == _mainWindow)
                {
                    _mainWindow = new MainWindow(CruciatusFactory.Root, By.Uid(TreeScope.Children, "ScoutXMainWindow"));
                    WindowWaiter.MainWindow = _mainWindow;
                }

                return _mainWindow;
            }
        }

        #endregion
    }
}