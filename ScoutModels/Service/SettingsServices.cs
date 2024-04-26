using ScoutDataAccessLayer.DAL;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Settings;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutModels.Service
{
    public class SettingsService : ISettingsService
    {
        /// <summary>
        /// Return the run options of the 'username' user (or current logged in user if username is null).
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public RunOptionSettingsDomain GetRunOptions(IDataAccess dataAccess = null, string username = null)
        {
            return GetRunOptionSettingsModel(dataAccess, username).RunOptionsSettings;
        }

        /// <summary>
        /// Return the run options of the 'username' user (or current logged in user if username is null).
        /// </summary>
        /// <param name="dataAccess"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        public RunOptionSettingsModel GetRunOptionSettingsModel(IDataAccess dataAccess = null, string username = null)
        {
            var da = dataAccess ?? XMLDataAccess.Instance;
            var userId = username ?? LoggedInUser.CurrentUserId;

            var runSettingsModel = new RunOptionSettingsModel(da, userId);
            runSettingsModel.GetRunOptionSettingsDomain(userId);
            return runSettingsModel;
        }

        /// <summary>
        /// Save the Run options (persistence).
        /// </summary>
        /// <param name="runOptionModel">The current user's run options to save.</param>
        /// <returns>true if successful, false otherwise</returns>
        public bool SaveRunOptions(RunOptionSettingsModel runOptionModel)
        {
            if (runOptionModel.SaveRunOptionSettings(LoggedInUser.CurrentUserId))
            {
                PostToMessageHub(LanguageResourceHelper.Get("LID_MSGBOX_RunOptionSuccessful"));
                MessageBus.Default.Publish(new Notification(MessageToken.RunSampleSettingsChanged, MessageToken.UserDefaultCellTypeChanged));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Copied from BaseViewModel. ToDo: Create a MessageHub service that can be injected.
        /// </summary>
        /// <param name="statusBarMessage"></param>
        /// <param name="messageType"></param>
        protected void PostToMessageHub(string statusBarMessage, MessageType messageType = MessageType.Normal)
        {
            MessageBus.Default.Publish(new SystemMessageDomain
            {
                IsMessageActive = true,
                Message = statusBarMessage,
                MessageType = messageType
            });
        }
    }
}