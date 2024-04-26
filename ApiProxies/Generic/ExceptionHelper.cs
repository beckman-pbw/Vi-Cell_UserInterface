using log4net;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Windows;

namespace ApiProxies.Generic
{
    public static class ExceptionHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void LogException(Exception exception, string errorCode)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Log.Error(exception.Message, exception);
                var sysMsg = new SystemMessageDomain
                {
                    IsMessageActive = true,
                    Message = LanguageResourceHelper.Get(errorCode),
                    MessageType = MessageType.System
                };
                MessageBus.Default.Publish(sysMsg);
            });
        }

        public static void HandleExceptions(Exception exception, string message)
        {
            Application.Current?.Dispatcher?.Invoke(() =>
            {
                Log.Error(exception.Message, exception);

                // Publish to status bar
                var sysMsg = new SystemMessageDomain {
					IsMessageActive = true, 
					Message = message, 
					MessageType = MessageType.System 
				};
                MessageBus.Default.Publish(sysMsg);
            });
        }
    }
}
