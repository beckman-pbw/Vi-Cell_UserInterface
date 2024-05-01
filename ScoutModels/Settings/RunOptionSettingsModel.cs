using HawkeyeCoreAPI.Facade;
using JetBrains.Annotations;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Interfaces;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using ScoutModels.Interfaces;

// ReSharper disable InconsistentNaming

namespace ScoutModels.Settings
{
    public class RunOptionSettingsModel : BaseNotifyPropertyChanged, ICloneable
    {
        #region Property

        public IDataAccess DataAccess { get; set; }

        public RunOptionSettingsDomain RunOptionsSettings
        {
            get { return GetProperty<RunOptionSettingsDomain>(); }
            set { SetProperty(value); }
        }

        public uint DefaultBPQC
        {
            get => RunOptionsSettings.DefaultBPQC;
            set => RunOptionsSettings.DefaultBPQC = value;
        }

        public SamplePostWash DefaultWash
        {
            get => RunOptionsSettings.DefaultWash;
            set => RunOptionsSettings.DefaultWash = value;
        }

        public RunOptionSettingsDomain SavedSettings
        {
            get { return GetProperty<RunOptionSettingsDomain>(); }
            set { SetProperty(value); }
        }

        public IList<GenericParametersDomain> GenericParameters
        {
            get { return GetProperty<IList<GenericParametersDomain>>(); }
            set { SetProperty(value); }
        }

        public IList<GenericParametersDomain> SavedParameters
        {
            get { return GetProperty<IList<GenericParametersDomain>>(); }
            set { SetProperty(value); }
        }

        public IList<SamplePostWash> WashList
        {
            get { return GetProperty<IList<SamplePostWash>>(); }
            set { SetProperty(value); }
        }

        public IList<CellTypeQualityControlGroupDomain> BpQcGroupList
        {
            get { return GetProperty<IList<CellTypeQualityControlGroupDomain>>(); }
            set { SetProperty(value); }
        }

        public uint Dilution
        {
            get { return GetProperty<uint>(); }
            set
            {
                if (value >= ApplicationConstants.MinimumDilutionFactor &&
                    value <= ApplicationConstants.MaximumDilutionFactor)
                    SetProperty(value);
            }
        }

        public uint SavedDisplayDigits
        {
            get { return LoggedInUser.CurrentUser?.DisplayDigits ?? 2; }
            set { SetProperty(value);}
        }

        public string DefaultDilution
        { 
            get => RunOptionsSettings.DefaultDilution;
            set => RunOptionsSettings.DefaultDilution = value;
        }

        public string DefaultSampleId
        { 
            get => RunOptionsSettings.DefaultSampleId;
            set => RunOptionsSettings.DefaultSampleId = value;
        }

		public string NumberOfImages
        { 
            get => RunOptionsSettings.NumberOfImages;
            set => RunOptionsSettings.NumberOfImages = value;
        }

        public string Username { get; set; } = "";

        #endregion

        #region Constructor

        public RunOptionSettingsModel(IDataAccess dataAccess, string username)
        {
            Username = username;
            DataAccess = dataAccess;
            RunOptionsSettings = new RunOptionSettingsDomain();
            SavedSettings = new RunOptionSettingsDomain();
            GenericParameters = new List<GenericParametersDomain>();
            SavedParameters = new List<GenericParametersDomain>();
            WashList = new List<SamplePostWash>(GetWashes());

            GetRunOptionSettingsDomain(username);
            GetGenericParameters(username);
            GetSavedSettingsDomain(username);

            Dilution = 1;

            if(uint.TryParse(RunOptionsSettings?.DefaultDilution, out var dilution))
                Dilution = dilution;

//            // CellHealth uses a dilution that is fixed in the A-Cup Sample workflow script.
//            Dilution = 4;
//            DefaultDilution = "4";

            if(!String.IsNullOrEmpty(username))
            {
                if(username == LoggedInUser.CurrentUserId)
                {
                    BpQcGroupList = new List<CellTypeQualityControlGroupDomain>(LoggedInUser.GetCtQcs()); // Don't make another backend call... should already have the updated list
                    if (BpQcGroupList.Count >= 2)
                        BpQcGroupList.RemoveAt(1); // Don't need Quality Controls in list
                }
                else
                {
                    BpQcGroupList = new List<CellTypeQualityControlGroupDomain>(/*CellTypeFacade.Instance.GetUserCtsOnly(username)*/);
                }
            }
            else
            {
                BpQcGroupList = new List<CellTypeQualityControlGroupDomain>();
            }
        }

        [Inject]
        public RunOptionSettingsModel(IDataAccess dataAccess, IUserService userService) : this(dataAccess, LoggedInUser.CurrentUserId)
        {

        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="copy"></param>
        public RunOptionSettingsModel(RunOptionSettingsModel copy)
        {
            Dilution = copy.Dilution;
            RunOptionsSettings = copy.RunOptionsSettings;
            BpQcGroupList = new List<CellTypeQualityControlGroupDomain>(copy.BpQcGroupList);
            GenericParameters = new List<GenericParametersDomain>(copy.GenericParameters);
            SavedDisplayDigits = copy.SavedDisplayDigits;
            SavedSettings = copy.SavedSettings;
            WashList = new List<SamplePostWash>(copy.WashList);
            SavedParameters = new List<GenericParametersDomain>(copy.SavedParameters);
            Username = copy.Username;
        }

        #endregion

        #region Method

        public static void SetConcTrailingPoint(uint selectedDigits)
        {
            switch (selectedDigits)
            {
                case 2:
                    ScoutUtilities.Misc.ConcDisplayDigits = TrailingPoint.Two;
                    break;
                case 3:
                    ScoutUtilities.Misc.ConcDisplayDigits = TrailingPoint.Three;
                    break;
                case 4:
                    ScoutUtilities.Misc.ConcDisplayDigits = TrailingPoint.Four;
                    break;
            }
        }

        public void GetRunOptionSettingsDomain(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                RunOptionsSettings = DataAccess.ReadConfigurationData<RunOptionSettingsDomain>(userId,
                    typeof(RunOptionSettingsDomain).Name.Replace("Domain", string.Empty),
                    out var userFound, true);

                try
                {
                    if (!userFound)
                    {
                        SaveRunOptionSettings(userId);
                        RunOptionsSettings.AppendSampleResultPath = FileSystem.GetDefaultExportPath(userId);
                        RunOptionsSettings.ExportSampleResultPath = FileSystem.GetDefaultExportPath(userId);
                        SaveRunOptionSettings(userId);
                    }
                    else
                    {
	                    // CellHealth uses a dilution that is fixed in the A-Cup Sample workflow script.
	                    Dilution = 4;
	                    DefaultDilution = "4";

                        bool save = false;
                        if (!FileSystem.IsFolderValidForExport(RunOptionsSettings.AppendSampleResultPath))
                        {
                            RunOptionsSettings.AppendSampleResultPath = FileSystem.GetDefaultExportPath(userId);
                            save = true;
                        }

                        if (!FileSystem.IsFolderValidForExport(RunOptionsSettings.ExportSampleResultPath))
                        {
                            RunOptionsSettings.ExportSampleResultPath = FileSystem.GetDefaultExportPath(userId);
                            save = true;
                        }
                        if (save)
                        {
                            SaveRunOptionSettings(userId);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"GetRunOptionSettingsDomain error ", e);
                }
            }
        }

        public void GetSavedSettingsDomain(string userId)
        {
            if (!string.IsNullOrEmpty(userId))
            {
                SavedSettings = DataAccess.ReadConfigurationData<RunOptionSettingsDomain>(userId,
                    typeof(RunOptionSettingsDomain).Name.Replace("Domain", string.Empty), 
                    out var userFound, true);

                try
                {
                    if (!userFound)
                        SaveRunOptionSettings(userId);
                }
                catch (Exception e)
                {
                    Log.Error($"GetSavedSettingsDomain error ", e);
                }
            }
        }

        public bool SaveRunOptionSettings(string userId)
        {
            DataAccess.WriteToConfigurationXML(RunOptionsSettings, userId, null);
            return true;
        }
     
        public IList<SamplePostWash> GetWashes()
        {
            return Enum.GetValues(typeof(SamplePostWash)).Cast<SamplePostWash>().ToList();
        }

        public void GetSavedParameters(string userId)
        {
            bool userFound;
            if (userId != null)
                SavedParameters = DataAccess.ReadConfigurationData<List<GenericParametersDomain>>(userId, "GenericParameters", out userFound);
        }

        public void GetGenericParameters(string userId)
        {
            bool userFound;
            if (userId != null)
                GenericParameters = DataAccess.ReadConfigurationData<List<GenericParametersDomain>>(userId, "GenericParameters", out userFound);

            foreach (var generic in GenericParameters)
            {
                switch (generic.ParameterID)
                {
                    case ResultParameter.eCellType:
                        generic.ParameterName=ScoutLanguageResources.LanguageResourceHelper.Get("LID_UsersLabel_CellType");
                        break;
                    case ResultParameter.eDilution:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Dilution");
                        break;
                    case ResultParameter.eWash:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_QMgmtHEADER_Workflow");
                        break;
                    case ResultParameter.eComment:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Label_Tag");
                        break;
                    case ResultParameter.eAnalysisDateTime:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_QCHeader_AnalysisDateTime");
                        break;
                    case ResultParameter.eReanalysisDateTime:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_QCHeader_ReAnalysisDateTime");
                         break;
                    case ResultParameter.eAnalysisBy:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Report_AnalysisBy");
                         break;
                    case ResultParameter.eReanalysisBy:
                        generic.ParameterName = ScoutLanguageResources.LanguageResourceHelper.Get("LID_Report_ReanalysisBy");
                         break;
                }
            }
        }

        public bool SaveGenericParameters(List<GenericParametersDomain> genericParametersDomains, string userId)
        {
            var genericParameters = new GenericParameters
            {
                GenericParametersList = genericParametersDomains
            };
            DataAccess.WriteToConfigurationXML(genericParameters, userId, null);
            return true;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetDisplayDigits(string userId, uint digits)
        {
            Log.Debug("SetDisplayDigits:: userId: " + userId);
            var hawkeyeError = HawkeyeCoreAPI.User.SetDisplayDigitsAPI(userId, digits);
            Log.Debug("SetDisplayDigits:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        public object Clone()
        {
            return new RunOptionSettingsModel(this);
        }

        #endregion
    }
}