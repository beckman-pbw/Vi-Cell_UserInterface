using HawkeyeCoreAPI;
using HawkeyeCoreAPI.Interfaces;
using Ninject.Extensions.Factory;
using Ninject.Modules;
using ScoutDataAccessLayer.DAL;
using ScoutDataAccessLayer.IDAL;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
 using ScoutModels.Security;
 using ScoutModels.Service;
 using ScoutModels.Settings;

 namespace ScoutModels.Ninject
{
    public class ScoutModelsModule : NinjectModule
    {
        private readonly ISecurityService _securityService;

        public ScoutModelsModule(ISecurityService securityService = null)
        {
            _securityService = securityService;
        }

        public override void Load()
        {
            Bind<IRunningWorkListModel>().To<RunningWorkListModel>().InSingletonScope();
            Bind<IInstrumentStatusService>().To<InstrumentStatusService>().InSingletonScope();
            Bind<IWorkListModel>().To<WorkListModel>().InSingletonScope();
            Bind<ICapacityManager>().To<CapacityManager>().InSingletonScope();
            Bind<ISystemStatus>().To<SystemStatus>().InSingletonScope();
            Bind<IAuditLog>().To<AuditLog>().InSingletonScope();
            Bind<IErrorLog>().To<ErrorLog>().InSingletonScope();
            Bind<IDisplayService>().To<DisplayService>().InSingletonScope();
            Bind<IHardwareSettingsModel>().To<HardwareSettingsModel>().InTransientScope();
            Bind<IApplicationStateService>().To<ApplicationStateService>().InSingletonScope();
            Bind<SecuredTask>().ToSelf().InTransientScope();
            Bind<IDataAccess>().ToConstant(XMLDataAccess.Instance);
            Bind<RunOptionSettingsModel>().ToSelf().InTransientScope();
            Bind<IScoutModelsFactory>().ToFactory();
            Bind<ISettingsService>().To<SettingsService>().InTransientScope();
            if (null != _securityService)
            {
                Bind<ISecurityService>().ToConstant(_securityService);
            }
            else
            {
                Bind<ISecurityService>().To<SecurityService>().InSingletonScope();
            }

        }
    }
}