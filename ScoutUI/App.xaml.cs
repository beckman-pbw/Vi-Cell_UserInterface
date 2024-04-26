using CommandLine;
using GrpcServer;
using log4net;
using Microsoft.Extensions.Configuration;
using Ninject;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Ninject;
using ScoutModels.Settings;
using ScoutServices.Ninject;
using ScoutServices.Watchdog;
using ScoutUI.Configuration;
using ScoutUI.Views;
using ScoutUI.Views.Dialogs;
using ScoutUI.Views.ScoutUIMain;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.UIConfiguration;
using ScoutViewModels;
using ScoutViewModels.ViewModels;
using ScoutViewModels.ViewModels.Dialogs;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutUI.Views.ucCommon;
using SplashScreen = ScoutUI.Views.ScoutUIMain.SplashScreen;
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace ScoutUI
{
    public partial class App
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // The global application Mutex used to ensure only a single instance of ScoutUI can be executed machine-wide
        private Mutex _mutex;

        private bool _showingFatalError;
        private bool _showedFatalError;
        private OpcUaGrpcServer _opcUaGrpcServer;
        private IKernel _container;
        private SplashScreen _splashScreen;
        private IConfigurationSource _cmdLineConfigSource;
        private IWatchdog _watchDog;
        private IHardwareManager _hardwareManager;
        private IObservable<InitializationState> _initializationObserver;
        private IDisposable _initializationSubscription;
        private StartupEventArgs _startupEventArgs;
        private IApplicationStateService _applicationStateService;
        private MainWindowViewModel _mainWindowViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Windows.Forms.Application.EnableVisualStyles(); 
            _startupEventArgs = e;
            RegisterUnhandledException();
            ConfigureLogging();
            // Do not put any new code above RegisterUnhandledException() or ConfigureLogging(). If you do and it fails, we have no 
            // logging to show what happened and must rely on Windows Event Viewer to get more info which is less than ideal.

            ParseCommandLineToConfiguration(e.Args);
            ConfigureContainer();
            
            // Start backend initialization and react to state changes.
            _hardwareManager = _container.Get<IHardwareManager>();
            _applicationStateService = _container.Get<IApplicationStateService>();
            // Get an instance of the MainWindowViewModel so it will register with the ApplicationStateService
            _mainWindowViewModel = _container.Get<MainWindowViewModel>();

            // Display Splash screen prior to starting backend initialization.
            _splashScreen = _container.Get<SplashScreen>();
            _splashScreen.Show();

            _initializationObserver = _hardwareManager.SubscribeStateChanges();
            _initializationSubscription = _initializationObserver.Subscribe(ProcessInitializationMessages);

            Task.Factory.StartNew(() =>
            {
                // Run the hardware initialization on a thread with lower priority to allow the Splash screen to be displayed faster (about 40% faster).
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.BelowNormal;
                    _hardwareManager.StartHardwareInitialize();
                }
                finally
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Normal;
                }
            });
        }

        private void ProcessInitializationMessages(InitializationState initializationState)
        {
            switch (initializationState)
            {
                case InitializationState.eInitializationComplete:
                    DispatcherHelper.ApplicationExecute(() =>
                    {
                        ComposeObjects();
                        Application_Startup(_startupEventArgs);
                        _applicationStateService.PublishStateChange(ApplicationStateEnum.Startup);
                        _container.Get<MainWindow>().Show();
                    });
                    break;
                
                case InitializationState.eInitializationFailed:
                    _applicationStateService.PublishStateChange(ApplicationStateEnum.Shutdown);
                    break;
                case InitializationState.eFirmwareUpdateFailed:
                    // ToDo: Add test resource message specific to exiting because of a firmware update failure
                    _applicationStateService.PublishStateChange(ApplicationStateEnum.Shutdown);
                    break;
            }
        }

        /// <summary>
        /// Parse the command line and set the values into an IConfigurationSource
        /// </summary>
        /// <param name="args"></param>
        private void ParseCommandLineToConfiguration(string[] args)
        {
            var commandLineProvider = new CommandLineProvider();
            _cmdLineConfigSource = new CommandLineSource(commandLineProvider);
            Parser.Default.ParseArguments<CommandLineOptions>(args)
                .WithParsed(o =>
                {
                    commandLineProvider.SetOption("OpcUa.Watchdog", o.NoWatchdog ? "False" : "True");
                });
        }

        protected override void OnExit(ExitEventArgs e)
        {
            App_OnExit(e);
            base.OnExit(e);
        }

        private void ConfigureContainer()
        {
            try
            {
                // Setup IConfiguration implementation
                var configBuilder = new ConfigurationBuilder();
                configBuilder.Sources.Add(_cmdLineConfigSource);
                configBuilder.AddJsonFile("appsettings.json");
                var config = configBuilder.Build();

                _container = new StandardKernel(new ScoutServiceModule(), new OpcUaGrpcModule(), new ScoutModelsModule(), new ScoutViewModelsModule(), new ScoutViewsModule(), new ScoutUtilitiesNinjectModule());
                _container.Bind<IConfiguration>().ToConstant(config);
                _container.Bind<MainWindow>().ToSelf().InSingletonScope();
                _container.Bind<MainWindowViewModel>().ToSelf().InSingletonScope();
                _container.Bind<IHardwareManager>().To<HardwareManager>().InSingletonScope();
                _container.Bind<SplashScreen>().ToSelf().InSingletonScope();
                _container.Bind<IWatchdog, ServerWatchdog>().To<ServerWatchdog>().InSingletonScope();
            }
            catch (Exception e)
            {
                Log.Error($"Unable to run ConfigureContainer()", e);
                throw;
            }
        }

        private void ComposeObjects()
        {
            _watchDog = _container.Get<IWatchdog>();
            Current.MainWindow = _container.Get<MainWindow>();
        }

        private void StartgRPCServer()
        {
            try
            {
                _opcUaGrpcServer = _container.Get<OpcUaGrpcServer>();
                _opcUaGrpcServer.StartServer();
            }
            catch (Exception e)
            {
                Log.Error($"Unable to StartgRPCServer()", e);
                throw;
            }
        }

        private void AddServerWatchList()
        {
            var settings = _container.Get<AutomationSettingsService>();
            if (settings.IsAutomationEnabled())
            {
                _watchDog.AddAllWatches();
            }
        }
      

        /// <summary>
        /// After the backend has initialized, this method is executed.
        /// It starts up the internal gRPC server and then the OPC server process.
        /// </summary>
        /// <param name="e">The event args initially passed to OnStartup().</param>
        private void Application_Startup(StartupEventArgs e)
        {
            StartgRPCServer();
            AddServerWatchList();

            try
            {
                _watchDog.Start();
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to start server watchdog()", ex);
                throw;
            }

            // By-passing multiple instance validation to force restart the application
            if (e.Args.Any() && e.Args.First() == ApplicationConstants.ForceRestartKey)
            {
                Task.Delay(3000).ContinueWith(t => ValidateForSingleScoutUiInstance());
            }
            else
            {
                ValidateForSingleScoutUiInstance();
            }
        }

        private static void ConfigureLogging()
        {
            try
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(UISettings.LoggingConfigurationSource));

                if (!Directory.Exists(UISettings.InstrumentPath + "Logs"))
                {
                    Directory.CreateDirectory(UISettings.InstrumentPath + "Logs");
                }

#if DEBUG
                GlobalContext.Properties["RuntimeMode"] = "DEBUG";
#else
                GlobalContext.Properties["RuntimeMode"] = "RELEASE";
#endif
            }
            catch (Exception e)
            {
                MessageBox.Show($"Unable to configure logging (App Startup). Error: '{e.Message}'");
            }
        }

        private void ValidateForSingleScoutUiInstance()
        {
            // ensures that irregardless of the privileges the application is running under it can use this Mutex. Mitigates System.UnauthorizedAccessException.
            var mutexSecuritySettings = new MutexSecurity();
            mutexSecuritySettings.AddAccessRule(new MutexAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), MutexRights.FullControl, AccessControlType.Allow));
            _mutex = new Mutex(true, @"Global\97A4651A-543A-40F6-A9B7-05BDFD6EF089", 
                out var firstTime, mutexSecuritySettings);

            if (firstTime)
            {
                firstTime &= _mutex.WaitOne();
            }

            if (!firstTime)
            {
                _mutex.Dispose();
                _mutex = null;
                
                var args = new DialogBoxEventArgs(LanguageResourceHelper.Get("LID_MSGBOX_InstanceAlreadyRunning"),
                    LanguageResourceHelper.Get("LID_MSGBOX_Information"), icon: MessageBoxImage.Information);
                var dialog = new DialogBox(new DialogBoxViewModel(args, null));
                dialog.ShowDialog();

                Environment.Exit(0);
            }
        }

        private void RegisterUnhandledException()
        {
            // Register to unhandled exception
            Current.DispatcherUnhandledException += OnCurrentDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
        }

        private void OnCurrentDispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            OnUnhandledException(e.Exception);
            e.Handled = true;
        }

        [HandleProcessCorruptedStateExceptions]
        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            OnUnhandledException(exception);
        }

        private void OnUnhandledException(Exception ex)
        {
            var exceptionMessages = Logger.GetAllExceptionMessages(ex);
            var msg = $"Unexpected Error Occurred{Environment.NewLine}'{exceptionMessages}'{Environment.NewLine}{Environment.NewLine}Application will be closed!";
            Log.Fatal(msg, ex);

            if (!_showingFatalError && !_showedFatalError)
            {
                _showingFatalError = true;
                
                var title = "Unexpected Error occurred!";
                var result = MessageBox.Show(msg, title, MessageBoxButton.OK);
                
                if (result == MessageBoxResult.OK && Current != null)
                {
                    Current.Shutdown();
                    Process.GetCurrentProcess().Kill();
                }
                
                _showedFatalError = true;
            }
            else if (Current != null && !_showingFatalError)
            {
                // We can register to "Application.Exit" event to find if application is
                // already being shut down, then this is not needed.
                Current.Shutdown();
                Process.GetCurrentProcess().Kill();
            }
        }

        private void App_OnExit(ExitEventArgs e)
        {
            try
            {
                if (Current != null)
                    Current.DispatcherUnhandledException -= OnCurrentDispatcherUnhandledException;
                if (AppDomain.CurrentDomain != null)
                    AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;

                _mutex?.ReleaseMutex();
                _mutex?.Dispose();

                // Stop Server Watchdog which kills all servers
                _watchDog?.ClearAllWatches();

                // Shutdown gRPC Server
                _opcUaGrpcServer?.ShutdownServer();

                //Dispose of any leftover Ninject objects
                _container.Dispose();

                if (e.ApplicationExitCode == Int32.MaxValue) // "Int32.MaxValue" is application default value for application restart
                {
                    var currentProcess = Process.GetCurrentProcess();
                    Process.Start(ResourceAssembly.Location);
                    currentProcess.Kill();
                    return;
                }
            }
            catch (Exception exitException)
            {
                Log.Debug($"Error while shutting down application", exitException);
                var currentProcess = Process.GetCurrentProcess();
                currentProcess.Kill();
            }
        }
    }
}
