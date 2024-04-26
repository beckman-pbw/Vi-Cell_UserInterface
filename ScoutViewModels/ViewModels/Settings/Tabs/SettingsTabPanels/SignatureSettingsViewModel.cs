using ApiProxies.Generic;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels;
using ScoutModels.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace ScoutViewModels.ViewModels.Tabs.SettingsPanel
{
    public class SignatureSettingsViewModel : BaseSettingsPanel
    {
        private bool _dirtyFlag;
        private SecuritySettingsModel _securitySettingsModel;

        #region Constructor

        public SignatureSettingsViewModel() : base()
        {
            IsSingleton = true;
            ListItemLabel = LanguageResourceHelper.Get("LID_CheckBox_Signatures");
            SecuritySettingsModel = new SecuritySettingsModel();
        }

        protected override void DisposeUnmanaged()
        {
            if (SecuritySettingsModel != null)
            {
                SecuritySettingsModel.SecuritySettingsDomain.Signatures.CollectionChanged -= OnSignaturesPropertyChanged;
                SecuritySettingsModel.Dispose();
            }
            base.DisposeUnmanaged();
        }

        public override void UpdateListItemLabel()
        {
            ListItemLabel = LanguageResourceHelper.Get("LID_CheckBox_Signatures");
        }

        #endregion

        #region Public Property

        public SecuritySettingsModel SecuritySettingsModel
        {
            get => _securitySettingsModel;
            set
            {
                _securitySettingsModel = value;
                _securitySettingsModel.SecuritySettingsDomain.Signatures.CollectionChanged += OnSignaturesPropertyChanged;
            }

        }

        public bool IsSecurityEnable
        {
            get { return GetProperty<bool>(); }
            set { SetProperty(value); }
        }

        public bool IsSaveButtonEnable
        {
            get { return GetProperty<bool>(); }
            set
            {
                SetProperty(value);
                SaveCommand.RaiseCanExecuteChanged();
            }
        }

        public SecuritySettingsDomain SecuritySettingsDomain
        {
            get { return SecuritySettingsModel.SecuritySettingsDomain; }
            set
            {
                SecuritySettingsModel.SecuritySettingsDomain = value;
                SecuritySettingsModel.SecuritySettingsDomain.Signatures.CollectionChanged += OnSignaturesPropertyChanged;
                IsSaveButtonEnable = CanPerformSave();
                NotifyPropertyChanged(nameof(SecuritySettingsDomain));
            }
        }

        public List<SignatureDomain> SavedSignatures
        {
            get { return GetProperty<List<SignatureDomain>>(); }
            set { SetProperty(value); }
        }

        private void OnSignaturesPropertyChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            _dirtyFlag = true;
            IsSaveButtonEnable = CanPerformSave();
        }

        protected override bool CanPerformSave()
        {
            if (!IsSecurityEnable || !_dirtyFlag)
                return false;
            
            var unsavedSigs = new List<(string meaning, string indicator)>();
            foreach (var sign in SecuritySettingsDomain.Signatures)
            {
                if (!string.IsNullOrEmpty(sign.SignatureIndicator) &&
                    !string.IsNullOrEmpty(sign.SignatureMeaning))
                {
                    unsavedSigs.Add((sign.SignatureMeaning, sign.SignatureIndicator));
                }
            }

            if(unsavedSigs.Count != SavedSignatures.Count)
                return true;

            for (var i = 0; i < SavedSignatures.Count; i++)
            {
                if(!SavedSignatures[i].SignatureMeaning.Equals(unsavedSigs[i].meaning) ||
                   !SavedSignatures[i].SignatureIndicator.Equals(unsavedSigs[i].indicator))
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region Commands

        protected override void PerformSave()
        {
            try
            {
                if (SecuritySettingsModel.ValidateUnsavedSettings(SecuritySettingsDomain))
                {
                    if (DeleteAllSignatures() && SaveCurrentSignatures())
                    {
                        IsSaveButtonEnable = _dirtyFlag = false;
                        SavedSignatures = SecuritySettingsModel.RetrieveSignatureDefinitions();
                        DialogEventBus.DialogBoxOk(this, LanguageResourceHelper.Get("LID_StatusBar_ResultRecordSaved"));
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionHelper.HandleExceptions(ex, LanguageResourceHelper.Get("LID_EXCEPTIONMSG_RunOptionSaveFailed"));
            }
        }

        #endregion

        #region Methods       

        public override void SetDefaultSettings()
        {
            IsSecurityEnable = IsSecurityTurnedOn && LoggedInUser.CurrentUserRoleId == UserPermissionLevel.eAdministrator;
            SavedSignatures = SecuritySettingsModel.RetrieveSignatureDefinitions();
            IsSaveButtonEnable = CanPerformSave();
        }

        private bool SaveCurrentSignatures()
        {
            foreach (var sign in SecuritySettingsDomain.Signatures)
            {
                if (!string.IsNullOrWhiteSpace(sign.SignatureIndicator))
                {
                    var signatureStatus = SecuritySettingsModel.AddSignatureDefinition(sign);
                    if (signatureStatus != HawkeyeError.eSuccess)
                    {
                        ApiHawkeyeMsgHelper.ErrorCreateSignature(signatureStatus, sign.SignatureIndicator);
                        return false;
                    }
                }
            }
            return true;
        }

        private bool DeleteAllSignatures()
        {
            var signList = SecuritySettingsModel.RetrieveSignatureDefinitions();
            foreach (var sign in signList)
            {
                if (!string.IsNullOrEmpty(sign.SignatureIndicator))
                {
                    var result = SecuritySettingsModel.RemoveSignatureDefinition(sign.SignatureIndicator,
                        (ushort)sign.SignatureIndicator.Length);
                    if (result != HawkeyeError.eSuccess)
                    {
                        ApiHawkeyeMsgHelper.ErrorCommon(result);
                        return false;
                    }
                }
            }
            return true;
        }

        #endregion
    }
}