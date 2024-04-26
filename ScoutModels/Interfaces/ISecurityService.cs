using ScoutDomains.Analysis;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    /// <summary>
    /// Injectable interface whose implementation wraps the static methods of SecurityManager, allowing
    /// unit tests to override the interface.
    /// </summary>
    public interface ISecurityService
    {
        UserPermissionLevel GetUserRole(string username);

        /// <summary>
        /// Validate the user credentials using the backend.
        /// </summary>
        /// <param name="username">UserId</param>
        /// <param name="password">Password</param>
        /// <returns></returns>
        bool LoginRemoteUser(string username, string password);
        void LogoutRemoteUser(string username);

        /// <summary>
        /// Used by OPC to not allow user's with expired passwords to login.
        /// </summary>
        /// <param name="username">UserId</param>
        /// <returns>true if the password is expired, false otherwise.</returns>
        bool IsPasswordExpired(string username);

        UserRecord GetUserRecord(string username);
    }
}