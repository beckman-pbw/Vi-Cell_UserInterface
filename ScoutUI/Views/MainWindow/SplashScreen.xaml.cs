using Ninject.Extensions.Logging;
using ScoutLanguageResources;
using ScoutModels.Interfaces;
using ScoutUI.Views.Dialogs;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using ScoutUtilities.UIConfiguration;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using ScoutModels.Settings;
using ScoutModels;

namespace ScoutUI.Views.ScoutUIMain
{
    public partial class SplashScreen
    {
        private readonly IHardwareManager _hardwareManager;
        private readonly DialogEventManager _dialogEventManager;
        private readonly ILogger _logger;

        public SplashScreen(IHardwareManager hardwareManager, ILogger logger, IScoutViewFactory viewFactory)
        {
            _hardwareManager = hardwareManager;
            _logger = logger;
            _dialogEventManager = viewFactory.CreateDialogEventManager(this);

            InitializeComponent();
            lblUIVersion.Content = "v" + UISettings.SoftwareVersion;
            Load();
        }

        #region Private Methods

        private void SetHardwareStatus(InitializationState initializationState)
        {
            switch (initializationState)
            {
                case InitializationState.eInitializationInProgress:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        txtLoading.Text = LanguageResourceHelper.Get("LID_Label_Initializing");
                    });
                    break;

                case InitializationState.eFirmwareUpdateInProgress:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        txtLoading.Text = LanguageResourceHelper.Get("LID_Label_UpdatingFirmware");
                        LblInitialTime.Content = LanguageResourceHelper.Get("LID_Label_Initial_Taketime");
                    });
                    break;

                case InitializationState.eInitializationFailed:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        txtLoading.Text = LanguageResourceHelper.Get("LID_Label_Initialization_Failed");
                        // If you need to inform the user what is going on, you need to do so
                        // in the HardwareManager before the Reactive event gets sent out 
                        // because this event (for eInitializationFailed) will cause the
                        // application to shut down (and we need to show a dialog BEFORE that).
                    });
                    break;

                case InitializationState.eInitializationComplete:
                    DispatcherHelper.ApplicationExecute(async () =>
                    {
                        await Task.Run(DisposeSplashScreen);
                    });
                    break;

                case InitializationState.eFirmwareUpdateFailed:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        txtLoading.Text = LanguageResourceHelper.Get("LID_Label_Firmware_Update_Failed");
                        // App.ProcessInitializationMessages is listening for this same message and will shutdown the application.
                    });
                    break;

                case InitializationState.eInitializationStopped_CarosuelTubeDetected:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        var args = new DialogBoxEventArgs(LanguageResourceHelper.Get("LID_MSGBOX_TubeDetected"), null,
                            DialogButtons.Continue);
                        DialogEventBus.DialogBox(this, args);
                    });

                    // Restart hardware initialization
                    Task.Run(() => _hardwareManager.StartHardwareInitialize());
                    break;
            }
        }

        private void Load()
        {
            PresentationTraceSources.Refresh(); // Enable WPF diagnostic logging
            LblInitialTime.Content = "";
            var subscription = _hardwareManager.SubscribeStateChanges().Subscribe(SetHardwareStatus);
            SetHardwareStatus(_hardwareManager.State ?? InitializationState.eInitializationInProgress);
        }

        private void DisposeSplashScreen()
        {
            Dispatcher.Invoke(Close);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _dialogEventManager.Dispose();
            base.OnClosing(e);
        }

        #endregion
    }
}
