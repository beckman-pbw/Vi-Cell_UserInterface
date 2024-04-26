// ***********************************************************************
// <copyright file="UserPermissionLevel.cs" company="Beckman Coulter Life Sciences">
//     Copyright (C) 2019 Beckman Coulter Life Sciences. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


using System.ComponentModel;

namespace ScoutUtilities.Enums
{
    public enum UserPermissionLevel
    {
        [Description("LID_UserRole_Normal")] eNormal = 0,
        [Description("LID_UserRole_Advanced")] eElevated,
        [Description("LID_UserRole_Admin")] eAdministrator,
        [Description("LID_UserRole_Service")] eService
    }

    public enum SwapUserRole
    {               
        eAdminOnly = 0,
        eServiceOnly,
        eServiceOrAdmin
    }

}