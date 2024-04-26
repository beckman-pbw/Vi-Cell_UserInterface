using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    public interface IAutomationSettingsService
    {
        AutomationConfig GetAutomationConfig();
        HawkeyeError SaveAutomationConfig(AutomationConfig config);
        HawkeyeError SaveAutomationConfig(bool isAutomationEnabled, bool isACupEnabled, uint port);
        
        bool IsValid(AutomationConfig config); 
        bool IsValid(bool isAutomationEnabled, bool isACupEnabled, uint port);

        bool CanSaveAutomationConfig(AutomationConfig newConfig, bool userIsAdmin, bool userIsService);
        bool CanSaveAutomationConfig(bool newIsAutomationEnabled, bool newIsACupEnabled, uint newPort, bool userIsAdmin, bool userIsService);
        bool IsAutomationEnabled();
    }
}