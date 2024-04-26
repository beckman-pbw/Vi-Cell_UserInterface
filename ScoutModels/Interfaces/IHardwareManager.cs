using System;
using ScoutUtilities.Enums;

namespace ScoutModels.Interfaces
{
    /// <summary>
    /// The HardwareManager manages the initialization of Scout's ApplicationSource (backend).
    /// This logic was extracted from the SplashScreen's WPF code behind. It also creates and
    /// cleans up the system events.
    /// </summary>
    public interface IHardwareManager : IDisposable
    {
        InitializationState? State { get; }
        void StartHardwareInitialize();
        void ListenSystemEvents();
        void StopListeningSystemEvents();
        IObservable<InitializationState> SubscribeStateChanges();
    }
}