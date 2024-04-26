using System;
using System.Windows;
using System.Windows.Threading;

namespace ScoutUtilities.Common
{
    public class DispatcherHelper
    {
        private static Dispatcher AppDispatcher => Application.Current?.Dispatcher;

        public static void ApplicationExecute(Action action, DispatcherPriority? priority = null)
        {
            if (AppDispatcher == null || AppDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                if (priority != null)
                {
                    AppDispatcher.Invoke(priority.Value, action);
                }
                else
                {
                    AppDispatcher.Invoke(action);
                }
            }
        }
    }
}