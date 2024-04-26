using ApiProxies.Generic;
using ScoutLanguageResources;
using ScoutModels;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;

namespace ScoutViewModels.Common
{
    public class IdleState
    {
        public static void ValidateIdleState(object parameter)
        {
            try
            {
                switch (LoggedInUser.CurrentUserRoleId)
                {
                    case UserPermissionLevel.eNormal:
                    case UserPermissionLevel.eElevated:
                    case UserPermissionLevel.eAdministrator:
                    case UserPermissionLevel.eService:
                        MessageBus.Default.Publish(new Notification(MessageToken.UpdateInactivityTimeout));
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_QUEUE_VALIDATE_IDLE_STATE"));
            }
        }
    }
}