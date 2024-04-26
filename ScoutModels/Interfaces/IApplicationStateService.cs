using System;
using ScoutModels.Common;
using ScoutUtilities.Enums;

namespace ScoutModels.Interfaces
{
    /// <summary>
    /// Service that manages the state of the application, such as Shutdown.
    /// </summary>
    public interface IApplicationStateService
    {
        IDisposable SubscribeStateChanges(Action<ApplicationStateChange> onNext);

        /// <summary>
        /// Notify all interested parties that a state change is occurring, currently just the application shutting down.
        /// </summary>
        /// <param name="newState">An enumeration, currently on Shutdown is available.</param>
        /// <param name="reason">Previously there was not a mechanism to display different messages when the ExitUiDialog
        /// dialog was displayed by the MainWindowViewModel. This mechanism will allow customization of the message. Such
        /// as if a firmware update failure occurs.
        /// Note: Use resource strings.</param>
        /// <param name="restart">In the event that the state change is a shutdown, indicates whether the application should restart.</param>
        void PublishStateChange(ApplicationStateEnum newState, string reason = null, bool restart = false);
    }
}