using HawkeyeCoreAPI;
using ScoutDataAccessLayer.DAL;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels.Admin;
using ScoutModels.Common;
using ScoutModels.Settings;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HawkeyeCoreAPI.Facade;
using ScoutDomains.User;
using ScoutUtilities;
using ScoutUtilities.UIConfiguration;
// ReSharper disable InconsistentNaming

namespace ScoutModels
{
    /// <summary>
    /// Performs caching of backend user data in a static UserDomain object.
    /// </summary>
    public class LoggedInUser
    {
        public static UserDomain CurrentUser { get { return _currentUser; } }
        
        public static string CurrentUserId => (null != _currentUser) ? _currentUser.UserID : string.Empty;
        
        public static UserPermissionLevel CurrentUserRoleId => _currentUser?.RoleID ?? UserPermissionLevel.eNormal;

        private static UserDomain _currentUser = null;
        private static List<QualityControlDomain> _allowedQcs = new List<QualityControlDomain>();

        private static List<CellTypeQualityControlGroupDomain> _ctQcList = new List<CellTypeQualityControlGroupDomain>();
        private static Subscription<Notification> QcCtSubscribtion;

        #region Public Methods
        public static List<CellTypeQualityControlGroupDomain> GetCtQcs() { return _ctQcList; }
        public static List<QualityControlDomain> GetAllowedQcs() { return _allowedQcs; }

        public static string CurrentExportPath { get; set; } = "";

        public static void UserLogin(string username)
        {
            var permission = UserPermissionLevel.eNormal;
            var res = User.GetCurrentUserAPI(ref username, ref permission);
            if (res == HawkeyeError.eSuccess)
            {
                _currentUser = new UserDomain(username);
                if (!string.IsNullOrEmpty(_currentUser.UserID))
                {
                    _currentUser.RoleID = permission;
                    GetUserValuesFromBackend(ref _currentUser);
                    SetSessionVariableDefaults(_currentUser);
                    QcCtSubscribtion = MessageBus.Default.Subscribe<Notification>(OnCellTypesQualityControlsUpdated);
                    return;
                }
            }
            _currentUser = null;
            return;
        }

        public static void UserLogout()
        {
            _currentUser = null;
            _allowedQcs.Clear();
            _ctQcList.Clear();
            MessageBus.Default.UnSubscribe(ref QcCtSubscribtion);
            return;
        }

        public static bool IsConsoleUserLoggedIn()
        {
            return (_currentUser != null);
        }

        public static bool NoLoggedInUser()
        {
            return !IsConsoleUserLoggedIn();
        }
        #endregion

        #region Private Methods

        private static void OnCellTypesQualityControlsUpdated(Notification msg)
        {
            if (string.IsNullOrEmpty(msg?.Token))
                return;
            if (_currentUser == null)
                return;

            if ((msg.Token.Equals(MessageToken.CellTypesUpdated) || msg.Token.Equals(MessageToken.QualityControlsUpdated)))
            {
                List<CellTypeDomain> cellTypes = CurrentUser.AssignedCellTypes;
                CellTypeFacade.Instance.GetAllowedCtQc(_currentUser.UserID, ref cellTypes, ref _allowedQcs, ref _ctQcList);
                _currentUser.AssignedCellTypes = cellTypes;
            }
        }

        private static void GetUserValuesFromBackend(ref UserDomain user)
        {           
            var userRecDomain = UserModel.GetUserRecord(user.UserID);
            user.UpdateFromRec(userRecDomain);

            var runSettingsModel = new RunOptionSettingsModel(XMLDataAccess.Instance, user.UserID);
            runSettingsModel.GetRunOptionSettingsDomain(user.UserID);
            CurrentExportPath = runSettingsModel.RunOptionsSettings.ExportSampleResultPath;

            try
            {
                if (!FileSystem.IsFolderValidForExport(CurrentExportPath))                
                    CurrentExportPath = FileSystem.GetDefaultExportPath(user.UserID);

                if (!FileSystem.EnsureDirectoryExists(CurrentExportPath))
                {
                    CurrentExportPath = FileSystem.GetDefaultExportPath("");
                    FileSystem.EnsureDirectoryExists(CurrentExportPath);
                }
            }
            catch (Exception ex)
            {                
                Debug.WriteLine(ex.Message);
            }

            List<CellTypeDomain> cellTypes = _currentUser.AssignedCellTypes;
            CellTypeFacade.Instance.GetAllowedCtQc(_currentUser.UserID, ref cellTypes, ref _allowedQcs, ref _ctQcList);
            _currentUser.AssignedCellTypes = cellTypes;
        }

        private static void SetSessionVariableDefaults(UserDomain user)
        {
            var userSession = user.Session;
            userSession.ResetSessionVariables();

            var defaultFromDate = DateTimeConversionHelper.DateTimeToStartOfDay(DateTime.Today.AddDays(ApplicationConstants.DefaultFilterFromDaysToSubtract));
            var defaultToDate = DateTimeConversionHelper.DateTimeToEndOfDay(DateTime.Today);

            // Filter Dialog
            userSession.SetVariable(SessionKey.FilterDialog_IsAllSelected, true);
            userSession.SetVariable(SessionKey.FilterDialog_SelectedCellTypeOrQualityControlGroup, null);
            userSession.SetVariable(SessionKey.FilterDialog_FromDate, defaultFromDate);
            userSession.SetVariable(SessionKey.FilterDialog_ToDate, defaultToDate);
            userSession.SetVariable(SessionKey.FilterDialog_SearchString, string.Empty);
            userSession.SetVariable(SessionKey.FilterDialog_TagSearchString, string.Empty);
            userSession.SetVariable(SessionKey.FilterDialog_FilteringItem, eFilterItem.eSampleSet);

            userSession.SetVariable(SessionKey.FilterDialog_SelectedUser, string.Empty);

            // Open Sample Dialog
            userSession.SetVariable(SessionKey.OpenSampleDialog_FromDate, defaultFromDate);
            userSession.SetVariable(SessionKey.OpenSampleDialog_ToDate, defaultToDate);

            // CompletedRuns VM
            userSession.SetVariable(SessionKey.CompletedRuns_FromDate, defaultFromDate);
            userSession.SetVariable(SessionKey.CompletedRuns_ToDate, defaultToDate);
            userSession.SetVariable(SessionKey.CompletedRuns_Comments, string.Empty);
            userSession.SetVariable(SessionKey.CompletedRuns_PrintTitle, $"{LanguageResourceHelper.Get("LID_Title_ViCellBluVersion")}{UISettings.SoftwareVersion}");

            // StorageTab VM
            userSession.SetVariable(SessionKey.StorageTab_FromDate, defaultFromDate);
            userSession.SetVariable(SessionKey.StorageTab_ToDate, defaultToDate);
        }

        #endregion

        #region Methods for Unit Testing ONLY

        /// <summary>
        /// This method is ONLY for running UNIT TESTS. If you are trying to access this
        /// method anywhere else: STOP IT! We need a way to set the current user for many
        /// unit tests. Should there be a better way to do this? Yes! Did we have time to
        /// make that better way? hmmm....
        /// Don't use this method out side of unit tests!!!!
        /// </summary>
        /// <param name="secretKey">This is to help ensure no one uses this method outside of a unit test</param>
        /// <param name="user"></param>
        public static void SetCurrentUserForUnitTestsOnly(string secretKey, UserDomain user)
        {
            if (!secretKey.Equals("Th1$Is@5ecr3tK3y___T311N0one!"))
            {
                throw new ArgumentException($"You should NOT be trying to use this method, '{nameof(SetCurrentUserForUnitTestsOnly)}()'!!!! SHAME!");
            }
            _currentUser = user;
        }

        #endregion
    }
}
