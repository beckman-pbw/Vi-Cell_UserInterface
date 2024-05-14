using System;
using System.Reactive.Subjects;
using System.Runtime.ExceptionServices;
using System.Threading;
using ApiProxies.Generic;
using Microsoft.Win32;
using Ninject.Extensions.Logging;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;


namespace ScoutModels
{
    public class HardwareManager : Disposable, IHardwareManager
    {
        private static readonly object InitLock = new object();
        private static bool _initialized;

        public static bool Initialized
        {
            get
            {
                lock (InitLock)
                {
                    return _initialized;
                }
            }

            private set
            {
                lock (InitLock)
                {
                    _initialized = value;
                }
            }
        }

        static HardwareManager()
        {
            Initialized = false;
        }

        private readonly Subject<InitializationState> _hardwareStateChangeSubject;
        private readonly ILogger _logger;
        private readonly IDialogCaller _dialogCaller;
        private static HardwareSettingsModel _hardwareSettingsModel = new HardwareSettingsModel();
        public static HardwareSettingsModel HardwareSettingsModel { get { return _hardwareSettingsModel; } }

        public bool IsFromHardware { get; set; }
        public string AssemblyName = "ScoutModels.dll";
        public string ClassName = "ScoutModels.Common.SystemEventsHandler";

        public InitializationState? State { get; private set; }
        
        public SystemEventsHandler SystemEventsHandler { get; set; }

        public HardwareManager(ILogger logger, IDialogCaller dialogCaller)
        {
            _logger = logger;
            _dialogCaller = dialogCaller;
            _hardwareStateChangeSubject = new Subject<InitializationState>();
            IsFromHardware = UISettings.IsFromHardware;
        }

        public void ListenSystemEvents()
        {
            SystemEventsHandler = InstanceCreator.CreateInstance<SystemEventsHandler>(AssemblyName, ClassName);
            SystemEvents.SessionSwitch += SystemEventsHandler.SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged += SystemEventsHandler.OnPowerChange;
            SystemEvents.SessionEnding += SystemEventsHandler.SystemEvents_SessionEnding;
        }

        public void StopListeningSystemEvents()
        {
            SystemEvents.SessionSwitch -= SystemEventsHandler.SystemEvents_SessionSwitch;
            SystemEvents.PowerModeChanged -= SystemEventsHandler.OnPowerChange;
            SystemEvents.SessionEnding -= SystemEventsHandler.SystemEvents_SessionEnding;
        }

        public IObservable<InitializationState> SubscribeStateChanges()
        {
            return _hardwareStateChangeSubject;
        }

        [HandleProcessCorruptedStateExceptions]
        public void StartHardwareInitialize()
        {
            _logger.Info($"StartHardwareInitialize: UI version: v {UISettings.SoftwareVersion}");

            try
            {
                HawkeyeCoreAPI.InitializeShutdown.InitializeAPI(out ushort instrumentType, IsFromHardware);
                _hardwareSettingsModel.InstrumentType = (InstrumentType)instrumentType;
//TODO: for debugging as Vi-Cell_BLU...
                //_hardwareSettingsModel.InstrumentType = InstrumentType.ViCELL_BLU_Instrument;

                State = InitializationState.eInitializationInProgress;
                _logger.Info("StartHardwareInitialize: InitializationState: " + State);
                _hardwareStateChangeSubject.OnNext(State.Value);
                var lastInitializationState = State;
                var done = false;
                
                while (! done)
                {
                    State = HawkeyeCoreAPI.InitializeShutdown.IsInitializationCompleteAPI();
                    
                    if (State != lastInitializationState)
                    {
                        _logger.Info("StartHardwareInitialize: InitializationState: " + State);

                        switch (State)
                        {
                            case InitializationState.eInitializationComplete:
                                ListenSystemEvents();
                                Initialized = done = true;
                                _hardwareSettingsModel.GetVersionInformation();
                                break;

                            case InitializationState.eInitializationFailed:
                                _logger.Error($"StartHardwareInitialize:eInitializationFailed");

                                // Show a dialog before broadcasting eInitializationFailed (that broadcast
                                // will shut the application down).
                                var msg = LanguageResourceHelper.Get("LID_MSGBOX_LowLevel_InitializeFail");
                                _dialogCaller.DialogBoxOk(this, msg);
                                done = true;
                                break;

                            case InitializationState.eInitializationStopped_CarosuelTubeDetected:
                                _logger.Warn("StartHardwareInitialize: Tube detected. InitializationState: " + State);
                                done = true;
                                break;

                            case InitializationState.eFirmwareUpdateFailed:
                                _logger.Error("StartHardwareInitialize: Firmware update failure. InitializationState: " + State);
                                var firmwareFailureMsg = LanguageResourceHelper.Get("LID_Label_Firmware_Update_Failed");
                                _dialogCaller.DialogBoxOk(this, firmwareFailureMsg);
                                done = true;
                                break;
                        }
                    }

                    if (State != null)
                    {
                        _hardwareStateChangeSubject.OnNext(State.Value);
                        lastInitializationState = State;
                    }

                    if (!done)
                    {
                        Thread.Sleep(500);
                    }
                }
            }
            catch (AccessViolationException ex)
            {
                _logger.Info(ex, "IsInitializationComplete:: InitializationState: AccessViolationException thrown");
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_SPLASHSCREEN_INITIALIZE_HAWKEYE"));
                _hardwareStateChangeSubject.OnNext(InitializationState.eInitializationFailed);
            }
            catch (Exception e)
            {
                _logger.Info(e, "IsInitializationComplete:: InitializationState: Exception thrown");
                _hardwareStateChangeSubject.OnNext(InitializationState.eInitializationFailed);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected override void DisposeUnmanaged()
        {
            StopListeningSystemEvents();
            base.DisposeUnmanaged();
        }
    }
}
