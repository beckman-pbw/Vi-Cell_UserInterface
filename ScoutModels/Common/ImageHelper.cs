using ScoutDomains.Analysis;
using ScoutUtilities.Enums;

namespace ScoutModels.Common
{
    public class ImageHelper
    {
        public static string GetUserIconPath(object o = null)
        {
            if (o == null) return string.Empty;

            if (o is UserPermissionLevel userPermission)
            {
                return GetUserIconPath(userPermission);
            }
            else if (o is UserDomain userDomain)
            {
                return userDomain.IsNullUser ? string.Empty : GetUserIconPath(userDomain.RoleID);
            }
            
            return "/Images/Normal-User.png";
        }

        public static string GetUserIconPath(UserPermissionLevel currentRole)
        {
            switch (currentRole)
            {
                case UserPermissionLevel.eNormal:
                    return "/Images/Normal-User.png";
                case UserPermissionLevel.eAdministrator:
                    return "/Images/Admin-User.png";
                case UserPermissionLevel.eElevated:
                    return "/Images/Admin-User-Add.png";
                case UserPermissionLevel.eService:
                    return "/Images/Services.png";
                default:
                    return "/Images/Normal-User.png";
            }
        }
    }
}
