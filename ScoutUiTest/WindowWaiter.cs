using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using NUnit.Framework;
using System.Windows.Automation;
using ScoutUiTest.Map;

namespace ScoutUiTest
{
    public delegate bool MatchWindow(AutomationElement element);

    /// <summary>
    /// Use this class to trigger an event that will open a window, and wait for that window to be opened.
    /// A single AutomationEventHandler is registered to listen for opening windows. Each window that is opened,
    /// generating an event, is checked against a list of these WindowWaiter instances. If it found, the signal
    /// in the WindowWaiter is triggered (Set()), which wakes up the thread (usually the test method) that was
    /// blocking on it. The test continues to execute, operating on the newly opened window.
    ///
    /// This is not a thread-safe class. If it needs to be, then change the RegisteredWindowEvents to a concurrent
    /// collection. Also, you cannot have two different threads wait on an event with the same name.
    /// </summary>
    public class WindowWaiter
    {
        private static MainWindow _mainWindow;
        private static readonly List<WindowWaiter> RegisteredWindowEvents = new List<WindowWaiter>();
        private static readonly List<WindowWaiter> RegisteredComponentEvents = new List<WindowWaiter>();
        private static bool _initialized = false;

        private readonly ManualResetEvent _eventWaitPoint = new ManualResetEvent(false);
        private readonly int _msMaxTimeToWait;
        private readonly string _windowName;
        private readonly MatchWindow _matchWindow;

        private WindowWaiter(string windowName, int msMaxTimeToWait, MatchWindow matchWindow)
        {
            _windowName = windowName;
            _msMaxTimeToWait = msMaxTimeToWait;
            _matchWindow = matchWindow;
        }

        public static MainWindow MainWindow
        {
            get => _mainWindow;
            set
            {
                if (null != _mainWindow)
                {
                    throw new Exception("Setting the MainWindow should only be called once.");
                }
                _mainWindow = value;

                // Listen for changes in application's WPF structure
                Automation.AddStructureChangedEventHandler(MainWindow.Instance, TreeScope.Children,
                    new StructureChangedEventHandler(OnStructureChanged));
            }
        }

        /// <summary>
        /// Create a WindowWaiter instance. If one already exists with the same name, the original is returned. For example:
        /// <code>
        /// var appWindowWaiter = WindowWaiter.CreateWindowWaiter("ScoutX application window",
        ///    60000, (AutomationElement element) => element.Current.AutomationId.Equals("ScoutXMainWindow"));
        /// </code>
        /// </summary>
        /// <param name="windowName">Name to be displayed in assertion failures for test.</param>
        /// <param name="msMaxTimeToWait">Number of milliseconds to wait for the window to open.
        /// You can make this twice the expected value as it will only wait this long if the test
        /// will fail. The test will proceed the moment the window opens as it is triggered by an
        /// automation event.</param>
        /// <param name="matchWindow">A predicate of type MatchWindow that indicates that the desired
        /// window opened. This can use the automationId/Uid, or the name, or some other identifying attribute.
        /// For example: (AutomationElement element, WindowWaiter windowWaiter)element.Current.AutomationId.Equals("ScoutXMainWindow")</param>
        /// <returns></returns>
        public static WindowWaiter CreateWindowWaiter(string windowName, int msMaxTimeToWait, MatchWindow matchWindow)
        {
            Initialize();

            var windowEvent = RegisteredWindowEvents.FirstOrDefault(ww => ww._windowName.Equals(windowName));
            if (null == windowEvent)
            {
                // Event does not yet exist. Create it
                windowEvent = new WindowWaiter(windowName, msMaxTimeToWait, matchWindow);
                RegisteredWindowEvents.Add(windowEvent);
                // System.Diagnostics.Debug.WriteLine($"Added {windowEvent._windowName} to window events.");
            }

            return windowEvent;
        }

        /// <summary>
        /// Use this to make sure a WPF component, such as a ListView, is created prior to using it. This will
        /// create a WindowWaiter instance. If one already exists with the same name, the original is returned. For example:
        /// <code>
        /// var appWindowWaiter = WindowWaiter.CreateWindowWaiter("ScoutX application window",
        ///    60000, (AutomationElement element) => element.Current.AutomationId.Equals("ScoutXMainMenu"));
        /// </code>
        /// </summary>
        /// <param name="windowName">Name to be displayed in assertion failures for test.</param>
        /// <param name="msMaxTimeToWait">Number of milliseconds to wait for the window to open.
        /// You can make this twice the expected value as it will only wait this long if the test
        /// will fail. The test will proceed the moment the window opens as it is triggered by an
        /// automation event.</param>
        /// <param name="matchWindow">A predicate of type MatchWindow that indicates that the desired
        /// window opened. This can use the automationId/Uid, or the name, or some other identifying attribute.
        /// For example: (AutomationElement element, WindowWaiter windowWaiter)element.Current.AutomationId.Equals("ScoutXMainWindow")</param>
        /// <returns></returns>
        public static WindowWaiter CreateComponentWaiter(string windowName, int msMaxTimeToWait, MatchWindow matchWindow)
        {
            Initialize();

            var windowEvent = RegisteredComponentEvents.FirstOrDefault(ww => ww._windowName.Equals(windowName));
            if (null == windowEvent)
            {
                // Event does not yet exist. Create it
                windowEvent = new WindowWaiter(windowName, msMaxTimeToWait, matchWindow);
                RegisteredComponentEvents.Add(windowEvent);
                // System.Diagnostics.Debug.WriteLine($"Added {windowEvent._windowName} to component events.");
            }

            return windowEvent;
        }

        /// <summary>
        /// Block on the mutex, and fail if timed out.
        /// </summary>
        public void Wait()
        {
            // System.Diagnostics.Debug.WriteLine($"{_windowName} waiting.");
            
            if (!_eventWaitPoint.WaitOne(_msMaxTimeToWait))
            {
                Assert.Fail($"{_windowName} did not open.");
            }

            // System.Diagnostics.Debug.WriteLine($"{_windowName} signaled and continuing execution.");
        }

        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            // Listen for windows opening, such as the main application window
            Automation.AddAutomationEventHandler(
                WindowPattern.WindowOpenedEvent,
                AutomationElement.RootElement,
                TreeScope.Children,
                (sender, e) =>
                {
                    SignalWaiter(RegisteredWindowEvents, sender);
                });

            _initialized = true;
        }

        /// <summary>
        /// Handles structure-changed events, such as opening the MainMenu ListView. If a WindowWaiter
        /// has been added, then this event handler will signal the waiter, unblocking the thread.
        /// </summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        /// <remarks>
        /// An exception can be thrown by the UI Automation core if the element disappears
        /// before it can be processed -- for example, if a menu item is only briefly visible. 
        /// This exception cannot be caught here because it crosses native/managed boundaries. 
        /// In the debugger, you can ignore it and continue execution. The exception does not cause
        /// a break when the executable is being run.
        /// </remarks>
        private static void OnStructureChanged(object sender, StructureChangedEventArgs e)
        {
            // System.Diagnostics.Debug.WriteLine($"WPF structured event {e.StructureChangeType} on sender {sender}.");
            if (e.StructureChangeType == StructureChangeType.ChildAdded || e.StructureChangeType == StructureChangeType.ChildrenBulkAdded)
            {
                SignalWaiter(RegisteredComponentEvents, sender);
            }
        }

        private static void SignalWaiter(List<WindowWaiter> registeredEvents, object sender)
        {
            var element = sender as AutomationElement;
            var match = false;
            using (var iter = registeredEvents.GetEnumerator())
            {
                WindowWaiter windowWaiter = null;
                while (!match && iter.MoveNext())
                {
                    windowWaiter = iter.Current;
                    if (windowWaiter != null)
                        match = windowWaiter._matchWindow(element);
                }

                // Signal to continue
                if (match)
                {
                    // System.Diagnostics.Debug.WriteLine($"Detected {windowWaiter._windowName} opening. Signaling.");
                    windowWaiter._eventWaitPoint.Set();
                }
            }
        }
    }
}