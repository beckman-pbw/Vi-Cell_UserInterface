using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using ScoutDomains.User;
using ScoutLanguageResources;
using ScoutUtilities.Interfaces;
using System.Linq;

namespace ScoutDomains.Analysis
{
    public class UserDomain : BaseNotifyPropertyChanged, IListItem
    {
        public UserDomain(string username = null)
        {
            if (!string.IsNullOrEmpty(username))
            {
                UserID = username;
            }
        }

        #region Properties and Fields

        public UserSession Session { get; set; } = new UserSession();

        public string DisplayName { get; set; }
        public string UserID { get; set; }
        public string ListItemLabel => UserID;
        public UserPermissionLevel RoleID { get; set; }
        public string RoleName { get; set; }
        private List<CellTypeDomain> _assignedCellTypes;
        public List<CellTypeDomain> AssignedCellTypes
        {
            get
            {
                var tmpList = new List<CellTypeDomain>();
                _assignedCellTypes?.ForEach(x => tmpList.Add((CellTypeDomain)x.Clone()));
                return tmpList;
            }
            set { _assignedCellTypes = value; }
        }
        public List<AnalysisDomain> AssignedApplicationTypes { get; set; }
        public string Comments { get; set; }
        public string Email { get; set; }
        private bool IsImmuneToPasswordExpiration => RoleID.Equals(UserPermissionLevel.eService) || UserID.Equals(ApplicationConstants.SilentAdmin);
        public bool IsChangePasswordEnable => !string.IsNullOrWhiteSpace(NewPassword) || !string.IsNullOrWhiteSpace(OldPassword) || !string.IsNullOrEmpty(ConfirmPassword);

        public UInt32 DisplayDigits { get; set; } = 2;

        // @todo - enable and use the values once write user record to DB is ready
        //public string LangCode { get; set; } = string.Empty;
        //public string SampleExportFolder { get; set; } = string.Empty;
        //public string CsvExportFolder { get; set; } = string.Empty;
        //public string DefaultResultFileName { get; set; } = string.Empty;
        //public string DefaultSampleId { get; set; } = string.Empty;
        //public Int16 DefaultImageSaveN { get; set; } = 1;
        //public SamplePostWash DefaultWash { get; set; } = SamplePostWash.NormalWash;
        //public Int16 DefaultDilution { get; set; } = 1;
        //public UInt32 DefaultCellTypeIndex { get; set; } = 0;
        //public bool SampleExportEnabled { get; set; } = false;
        //public bool CsvExportEnabled { get; set; } = false;
        //public bool ExportPdfEnabled { get; set; } = false;

        public bool IsNullUser => string.IsNullOrEmpty(UserID);

        public string OldPassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsChangePasswordEnable));
            }
        }

        public string NewPassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsChangePasswordEnable));
            }
        }

        public string ConfirmPassword
        {
            get { return GetProperty<string>(); }
            set
            {
                SetProperty(value);
                NotifyPropertyChanged(nameof(IsChangePasswordEnable));
            }
        }

        public bool IsEnabled
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsFastModeEnabled
        {
            get { return RoleID == UserPermissionLevel.eAdministrator || 
                         RoleID == UserPermissionLevel.eService || GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        #endregion

        #region Methods

        public bool IsServiceUser()
        {
            var serviceUser = ApplicationConstants.ServiceUser;
            var userId = UserID ?? string.Empty;
            return serviceUser.Equals(userId);
        }
        
        /// <summary>
        /// Compare two users, where either could be null.
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        /// <returns>true if both UserId(s) are the same. If either are null or different, then false.</returns>
        public static bool SameUser(string user1, string user2)
        {
            return null != user1 && null != user2 && user1.Equals(user2);
        }

        public void UpdateFromRec(ScoutUtilities.Structs.UserRecord userRec)
        {
            //
            // moved from individual calls for each value
            //
            DisplayName = userRec.DisplayName;
            Comments = userRec.Comments;
            Email = userRec.Email;
            IsFastModeEnabled = userRec.AllowFastMode;
            DisplayDigits = userRec.DisplayDigits;
            RoleID = userRec.PermissionLevel;
            RoleName = LanguageResourceHelper.Get(GetEnumDescription.GetDescription(RoleID));

            // @todo - update all values retrieved 
            //         this requires the use of write user record to DB
            //
            //LangCode = userRec.LangCode;
            //SampleExportFolder = userRec.SampleExportFolder;
            //CsvExportFolder = userRec.CsvExportFolder;
            //DefaultResultFileName = userRec.DefaultResultFileName;
            //DefaultSampleId = userRec.DefaultSampleName;
            //DefaultImageSaveN = userRec.DefaultImageSaveN;
            //DefaultDilution = userRec.DefaultDilution;
            //DefaultCellTypeIndex = userRec.DefaultCellTypeIndex;
            //ExportPdfEnabled = userRec.ExportPdfEnabled;
            //DefaultWash = (SamplePostWash)userRec.DefaultWashType;
        }

        #endregion
    }
}
