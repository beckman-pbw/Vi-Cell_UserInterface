using System;
using HawkeyeCoreAPI;
using ScoutDomains.Analysis;
using ScoutModels.Admin;
using ScoutModels.Interfaces;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Security
{
    /// <summary>
    /// Thin wrapper that calls SecurityManager DLL wrapper. Exists so that the backend can be mocked out.
    /// </summary>
    public class SecurityService : ISecurityService
    {
        public UserPermissionLevel GetUserRole(string username)
        {
            UserPermissionLevel level = UserPermissionLevel.eNormal;
            if (User.GetUserPermissionLevelAPI(username, ref level) != HawkeyeError.eSuccess)
            {
                // @todo - log error or ???
            }
            return level;
        }

        public bool LoginRemoteUser(string username, string password)
        {
            return HawkeyeError.eSuccess == User.LoginRemoteUserAPI(username, password);
        }

        public void LogoutRemoteUser(string username)
        {
            User.LogoutRemoteUserAPI(username);
        }

        public bool IsPasswordExpired(string username)
        {
            return HawkeyeError.eSuccess != User.IsPasswordExpiredAPI(username, out var expired) || expired;
        }

        public UserRecord GetUserRecord(string username)
        {
           return UserModel.GetUserRecord(username);
        }
    }
}