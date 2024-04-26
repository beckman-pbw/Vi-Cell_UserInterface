using HawkeyeCoreAPI.Facade;
using Ninject.Modules;
using ScoutDataAccessLayer.DAL;
using ScoutDataAccessLayer.IDAL;
using ScoutModels;
using ScoutModels.Interfaces;
using ScoutModels.Settings;
using ScoutServices.Interfaces;
using ScoutServices.Service.ConcentrationSlope;

namespace ScoutServices.Ninject
{
    public class ScoutServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<ILockManager>().To<LockManager>().InSingletonScope();
            Bind<IOpcUaCfgManager>().To<OpcUaCfgManager>().InSingletonScope();
            Bind<IDbSettingsService>().To<DbSettingsModel>().InSingletonScope();
            Bind<ISmtpSettingsService>().To<SmtpSettingsModel>().InSingletonScope();
            Bind<IAutomationSettingsService>().To<AutomationSettingsService>().InSingletonScope();
            Bind<ISampleResultsManager>().To<SampleResultsManager>().InSingletonScope(); // needs to be singleton for IDisposable
            Bind<ISampleProcessingService>().To<SampleProcessingService>().InSingletonScope();
            Bind<ICellTypeManager>().To<CellTypeManager>().InSingletonScope();
            Bind<IConfigurationManager>().To<ConfigurationManager>().InSingletonScope();
            Bind<IScheduledExportsService>().To<ScheduledExportsService>().InSingletonScope();
            Bind<IUserService>().To<UserService>().InSingletonScope();
            Bind<IAcupConcentrationService>().To<AcupConcentrationService>().InSingletonScope();
            Bind<IConcentrationSlopeService>().To<ConcentrationSlopeService>().InSingletonScope();
            Bind<CellTypeFacade>().ToConstant(CellTypeFacade.Instance);
            Bind<IMaintenanceService>().To<MaintenanceService>().InSingletonScope();
        }
    }
}
