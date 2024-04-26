using System;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;
using NUnit.Framework;

namespace ScoutUiTest
{
    public static class TestClassHelper
    {
        private static readonly ManualResetEvent AppOpened = new ManualResetEvent(false);

        // Wrapper for waiting for the main based on timeouts
        private static readonly Stopwatch Timer = new Stopwatch();
        private const string DeployFolder = @"..\..\..\..\..\..\Instrument\Software\";
        private const int WaitTime = 60000;

        #region Public Methods and Operators

        public static void Cleanup(ScoutXApp application)
        {
            Automation.RemoveAllEventHandlers();
            var isClosed = application.Close() || application.Kill();
            Assert.IsTrue(isClosed, "Failed to terminate application ScoutX.");
        }

        public static void Initialize(out ScoutXApp application, int waitTime = WaitTime)
        {
            var appsFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appPath = Path.Combine(appsFolder ?? throw new InvalidOperationException(), DeployFolder, "ViCellBLU_UI.exe");
            Assert.IsTrue(File.Exists(appPath));
            application = new ScoutXApp(appPath);
            var appWindowWaiter = WindowWaiter.CreateWindowWaiter("ScoutX application window",
                60000, (AutomationElement element) => element.Current.AutomationId.Equals("ScoutXMainWindow"));
            application.Start();
            appWindowWaiter.Wait();
        }
        #endregion
    }
}
