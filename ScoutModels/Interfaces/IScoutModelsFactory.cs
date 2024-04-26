using ScoutModels.Security;
using ScoutModels.Settings;

namespace ScoutModels.Interfaces
{
    public interface IScoutModelsFactory
    {
        SecuredTask CreateSecuredTask();
        RunOptionSettingsModel CreateRunOptionSettingsModel(string username);
    }
}