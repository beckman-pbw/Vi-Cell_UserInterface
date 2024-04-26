using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutModels.Settings;

namespace ScoutModels.Service
{
    public interface ISettingsService
    {
        /// <summary>
        /// Return the run options of the 'username' user (or current logged in user if username is null).
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        RunOptionSettingsDomain GetRunOptions(IDataAccess dataAccess = null, string username = null);

        /// <summary>
        /// Return the run options of the 'username' user (or current logged in user if username is null).
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        RunOptionSettingsModel GetRunOptionSettingsModel(IDataAccess dataAccess = null, string username = null);

        /// <summary>
        /// Save the Run options (persistence).
        /// </summary>
        /// <param name="runOptionModel">The current user's run options to save.</param>
        /// <returns>true if successful, false otherwise</returns>
        bool SaveRunOptions(RunOptionSettingsModel runOptionModel);
    }
}