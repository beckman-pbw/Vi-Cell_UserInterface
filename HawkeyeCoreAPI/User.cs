using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ScoutDomains.Analysis;

namespace HawkeyeCoreAPI
{
    public static partial class User
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region API_Declarations

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError LoginConsoleUser(string userName, string password);

        [DllImport("HawkeyeCore.dll")]
        static extern void LogoutConsoleUser();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError LoginRemoteUser(string userName, string password);

        [DllImport("HawkeyeCore.dll")]
        static extern void LogoutRemoteUser(string userName);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError RemoveUser(string name);
       
        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ChangeUserPassword(string name, string password);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ChangeMyPassword(string oldPassword, string newx);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AddUser(string name, string displayName, string password, UserPermissionLevel permission);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SwapUser(string userName, string password, SwapUserRole swapRole);

        [DllImport("HawkeyeCore.dll")]
        static extern IntPtr GenerateHostPassword(IntPtr securitykey);
        
        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ValidateMe(string password);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ValidateUserCredentials(string userName, string password);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ValidateLocalAdminAccount(string userName, string password);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError EnableUser(string name, bool enabled);


        /// <summary>
        /// Indicates of the given user's password is expired. 
        /// </summary>
        /// <param name="name">The name of the user to check if the password is expired</param>
        /// <param name="expired">True if the password is expired, False if not</param>
        /// <returns>
        /// eSuccess if the expired parameter was set
        /// eEntryNotFound if the given user was not found
        /// eNotPermittedAtThisTime if the user is not valid
        /// </returns>
        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError IsPasswordExpired(string name, out bool expired);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError AdministrativeUserEnable(string administrator_account, string administrator_password, string user_account); //working
                                                                                                                                               /// <summary>
        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError ChangeUserPermissions(string name, UserPermissionLevel permission);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserFolder(string name, string folder);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetUserList(bool only_enabled, out IntPtr ptrUserList, out UInt32 numUsers);   

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetCurrentUser(out IntPtr name, out UserPermissionLevel permission);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError GetUserPermissionLevel(string uName, out UserPermissionLevel permission);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        public static extern HawkeyeError ChangeUserDisplayName(string name, string displayname);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserPasswordExpiration(UInt16 days);

        [DllImport("HawkeyeCore.dll")]
        static extern void GetUserPasswordExpiration(out UInt16 days);

        [DllImport("HawkeyeCore.dll")]
        static extern void GetUserInactivityTimeout(out UInt16 minutes);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserInactivityTimeout(UInt16 minutes);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError LogoutUser_Inactivity();

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetDisplayDigits(string username, uint digits);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetUserEmail(string username, IntPtr email);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError SetUserComment(string username, IntPtr comment);

        [DllImport("HawkeyeCore.dll")]
        [MustUseReturnValue("Use HawkeyeError")]
        static extern HawkeyeError SetUserFastMode(string username, bool enabled);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern HawkeyeError GetUserRecord(string username, out IntPtr userRecord);

        [DllImport(ApplicationConstants.DllName)]
        [MustUseReturnValue(ApplicationConstants.Justification)]
        static extern void FreeUserRecord(IntPtr userRecord);

        #endregion


        #region API_Calls

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ChangeUserPasswordAPI(string name, string password)  
        {
            return ChangeUserPassword(name, password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AddUserAPI(string name, string displayName, string password, UserPermissionLevel permission)
        {
            return AddUser(name, displayName, password, permission);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SwapUserAPI(string name, string password, SwapUserRole swapRole) 
        {
            return SwapUser(name, password, swapRole);
        }

        public static void LogoutConsoleUserAPI()
        {
            LogoutConsoleUser();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError LoginConsoleUserAPI(string username, string password) 
        {
            return LoginConsoleUser(username, password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static void LogoutRemoteUserAPI(string username)
        {
            LogoutRemoteUser(username);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError LoginRemoteUserAPI(string username, string password)
        {
            return LoginRemoteUser(username, password);
        }


        public static HawkeyeError ChangeMyPasswordAPI(string oldPassword, string newPassword) 
        {
            return ChangeMyPassword(oldPassword, newPassword);
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateMeAPI(string password)
        {
            return ValidateMe(password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateUserCredentialsAPI(string uname, string password)
        {
            return ValidateUserCredentials(uname, password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ValidateLocalAdminAccountAPI(string uname, string password)
        {
            return ValidateLocalAdminAccount(uname, password);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError IsPasswordExpiredAPI(string name, out bool expired)
        {
            expired = true;
            return IsPasswordExpired(name, out expired);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError EnableUserAPI(string name, bool enabled) 
        {
            return EnableUser(name, enabled);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError AdministrativeUserEnableAPI(string administrator_account, string administrator_password, string user_account) 
        {
            return AdministrativeUserEnable(administrator_account, administrator_password, user_account);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ChangeUserPermissionAPI(string name, UserPermissionLevel permission)
        {
            return ChangeUserPermissions(name, permission);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserFolderAPI(string name, string folder)
        {
            return SetUserFolder(name, folder);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetCurrentUserAPI(ref string name, ref UserPermissionLevel permission)
        {
            IntPtr ptrName;
            var he = GetCurrentUser(out ptrName, out permission);
            name = ptrName.ToSystemString();
            GenericFree.FreeCharBufferAPI(ptrName);
            return he;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetUserPermissionLevelAPI(string userName, ref UserPermissionLevel permission)
        {
            return GetUserPermissionLevel(userName, out permission);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError GetUserListAPI(bool only_enabled, ref List<string> userList, ref UInt32 numUsers)
        {
            IntPtr ptrUserList;
            var he = GetUserList(only_enabled, out ptrUserList, out numUsers);
            if (he.Equals(HawkeyeError.eSuccess) && numUsers > 0 && ptrUserList != IntPtr.Zero)
            {
                var ptrUserListArray = new IntPtr[numUsers];
                Marshal.Copy(ptrUserList, ptrUserListArray, 0, (int)numUsers);

                for (int i = 0; i < numUsers; i++)
                    userList.Add(ptrUserListArray[i].ToSystemString());
            }

            GenericFree.FreeListOfCharBuffers(ptrUserList, numUsers);
            return he;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserDisplayNameAPI(string name, string displayName)
        {
            return ChangeUserDisplayName(name, displayName);
        }

        public static string GenerateHostPasswordAPI(string securityKey)
        {
            var keyPtr = securityKey.ToIntPtr();
            var securityPtr = GenerateHostPassword(keyPtr);
            keyPtr.ReleaseIntPtr();
            var password = securityPtr.ToSystemString();
            GenericFree.FreeCharBufferAPI(securityPtr);
            return password;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError RemoveUserAPI(string name) 
        {
            return RemoveUser(name);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static bool IsPasswordExpiredAPI(string username)
        {
            try
            {
                var result = IsPasswordExpired(username, out bool expired);
                if (result != HawkeyeError.eSuccess)
                {
                    expired = true;
                }
                return expired;
            }
            catch
            {
                return true;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserPasswordExpirationAPI(UInt16 days)
        {
            return SetUserPasswordExpiration(days);
        }

        public static void GetUserInactivityTimeoutAPI(out ushort minutes)
        {
            GetUserInactivityTimeout(out minutes);
        }

        public static void GetUserPasswordExpirationAPI(out ushort days)
        {
            GetUserPasswordExpiration(out days);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError LogoutUser_InactivityAPI() 
        {
            return LogoutUser_Inactivity();
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserInactivityTimeoutAPI(ushort minutes) 
        {
            return SetUserInactivityTimeout(minutes);
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetDisplayDigitsAPI(string username, uint digits)
        {
            return SetDisplayDigits(username,digits);
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static bool SetUserEmailApi(string username, string email)
        {
            try
            {
                var emailIntPtr = email.ToIntPtr();
                var result = SetUserEmail(username, emailIntPtr);
                Marshal.FreeCoTaskMem(emailIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"Failed to set user email: {result}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to set user email", e);
                return false;
            }
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static bool SetUserCommentApi(string username, string comment)
        {
            try
            {
                var commentIntPtr = comment.ToIntPtr();
                var result = SetUserComment(username, commentIntPtr);
                Marshal.FreeCoTaskMem(commentIntPtr);

                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"Failed to set user comment: {result}");
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to set user comment", e);
                return false;
            }
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserFastModeApi(string username, bool enabled)
        {
            return SetUserFastMode(username, enabled);
        }
        #endregion

        [MustUseReturnValue(ApplicationConstants.Justification)]
        public static UserRecord GetUserRecordApi(string username)
        {
            try
            {
                var result = GetUserRecord(username, out IntPtr userRecordIntPtr);
                if (result != HawkeyeError.eSuccess)
                {
                    Log.Warn($"{nameof(GetUserRecordApi)} error: {result}");
                    return new UserRecord();
                }
                var tmpRec = (UserRecord)Marshal.PtrToStructure(userRecordIntPtr, typeof(UserRecord));
                UserRecord userRec = new UserRecord();

                userRec.UserName = tmpRec.UserName;
                userRec.DisplayName = tmpRec.DisplayName;
                userRec.Comments = tmpRec.Comments;
                userRec.Email = tmpRec.Email;
                userRec.SampleExportFolder = tmpRec.SampleExportFolder;
                userRec.CsvExportFolder = tmpRec.CsvExportFolder;
                userRec.LangCode = tmpRec.LangCode;
                userRec.DefaultResultFileName = tmpRec.DefaultResultFileName;
                userRec.DefaultSampleName = tmpRec.DefaultSampleName;
                userRec.DefaultImageSaveN = tmpRec.DefaultImageSaveN;
                userRec.DefaultWashType = tmpRec.DefaultWashType;
                userRec.DefaultDilution = tmpRec.DefaultDilution;
                userRec.DefaultCellTypeIndex = tmpRec.DefaultCellTypeIndex;
                userRec.ExportPdfEnabled = tmpRec.ExportPdfEnabled;
                userRec.DisplayDigits = tmpRec.DisplayDigits;
                userRec.AllowFastMode = tmpRec.AllowFastMode;
                userRec.PermissionLevel = tmpRec.PermissionLevel;

                FreeUserRecord(userRecordIntPtr);
                return userRec;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get sample column settings", e);
            }
            return new UserRecord();
        }

    }
}
