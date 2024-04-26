using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NUnit.Framework;
using ScoutUiTest.Map;
using ScoutUiTest.TabModel;
using ScoutUiTest.TabOrder;
using Winium.Cruciatus;
using Winium.Cruciatus.Extensions;
using Winium.Cruciatus.Settings;

namespace ScoutUiTest
{
    /// <summary>
    /// Use Winium to start instance of ScoutX and open the settings form.
    /// Traverse all the form widgets and assert that performing a tab key
    /// traverses the form in the correct order
    /// </summary>
    [TestFixture]
    public class TabOrderTest
    {
        #region Fields

        private ScoutXApp _application;
        private MainWindow _mainWindow;
        private TabGroup _topLevelTabGroup;

        #endregion

        #region Public Methods and Operators

        [OneTimeSetUp]
        public void FixtureSetUp()
        {
            TestClassHelper.Initialize(out this._application);
            Thread.Sleep(2000);
            _mainWindow = _application.MainWindow;
            _topLevelTabGroup = new TopLevelBuilder().Build();

        }

        [OneTimeTearDown]
        public void FixtureTearDown()
        {
            TestClassHelper.Cleanup(this._application);
        }

        /* DO NOT DELETE - Jenkins is not respecting the Ignore. Will get this test working!
        [Test, Ignore("Needs work")]
        public void HomeViewTabOrderingSimpleSuccess()
        {
            var homeViewTopLevelTabGroup = _topLevelTabGroup.Clone();
            var homeViewTabGroup = new HomeViewBuilder().Build();
            homeViewTopLevelTabGroup.AddTabPoint(homeViewTabGroup);
            //_application.MainWindow.OpenSettings();
            CruciatusFactory.Settings.KeyboardSimulatorType = KeyboardSimulatorType.BasedOnInputSimulatorLib;
            CruciatusFactory.Keyboard.SendTab();
            Assert.IsTrue(TabVerifier.IsValid(homeViewTopLevelTabGroup));
        }
        */

        #endregion
    }
}
