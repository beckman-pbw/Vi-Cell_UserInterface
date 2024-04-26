using System;
using System.Threading;
using ScoutUtilities.Enums;

namespace ScoutUtilities.CustomEventArgs
{
    /// <summary>
    /// When changing to AD security mode we need to validate the configuration
    /// before changing security to AD. The group names are validated as
    /// valid groups on the server already. We also need to verify a given
    /// user will have admin rights. 
    /// </summary>
    public class AdCfgValidation
    {
        public string domain = "";
        public string server = "";
        public int port = 0;
        public string adminGroup = ""; /// Used to verify a user as an admin
    }

    public delegate bool LoginProc(AdCfgValidation cfg);

    public class ActiveDirectoryConfigDialogEventArgs : BaseDialogEventArgs
    {
        public object ActiveDirectoryDomain { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public ActiveDirectoryConfigDialogEventArgs(object activeDirectoryDomain, bool isModal = true, DialogLocation dialogLocation = DialogLocation.CenterApp, bool sizeToContent = false, int closeButtonSize = 40, bool fadeBg = true, string dialogIconPath = null, bool dialogIconIsSquare = false, Action<bool?> onCloseCallbackAction = null, CancellationToken? cancellationToken = null) : base(isModal, dialogLocation, sizeToContent, closeButtonSize, fadeBg, dialogIconPath, dialogIconIsSquare, onCloseCallbackAction, cancellationToken)
        {
            ActiveDirectoryDomain = activeDirectoryDomain;
        }
    }
}