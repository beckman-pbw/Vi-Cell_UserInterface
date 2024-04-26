using HawkeyeCoreAPI.Facade;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutModels
{
    public class SecuritySettingsModel : BaseDisposableNotifyPropertyChanged
    {
        #region Property

        private SecuritySettingsDomain _securitySettings;

        public ReviewModel ReviewModel { get; set; }

        public SecuritySettingsDomain SecuritySettingsDomain
        {
            get { return _securitySettings ?? (_securitySettings = SecuritySettingLoad()); }
            set { _securitySettings = value; }
        }

        #endregion

        #region Constructor

        public SecuritySettingsModel()
        {
            ReviewModel = new ReviewModel();
        }

        public SecuritySettingsModel(ReviewModel reviewModel)
        {
            ReviewModel = reviewModel;
        }

        protected override void DisposeUnmanaged()
        {
            ReviewModel?.Dispose();
        }

        #endregion

        #region Methods

        #region Validation Methods

        public static bool ValidateUnsavedSettings(SecuritySettingsDomain securitySettingsDomain)
        {
            return ValidateSignatures(securitySettingsDomain.Signatures.ToList());
        }

        public static bool ValidateInactiveTimeOut(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_InactivityTimeoutBlank"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_InactivityTimeoutBlank"));
                return false;
            }

            if (Convert.ToInt32(value) >= ApplicationConstants.MinimumInactivityTimeoutMins &&
                Convert.ToInt32(value) <= ApplicationConstants.MaximumInactivityTimeoutMins)
            {
                return true;
            }

            var message = string.Format(LanguageResourceHelper.Get("LID_Label_InActivityTimeoutLimit"),
                Misc.ConvertToString(ApplicationConstants.MinimumInactivityTimeoutMins),
                Misc.ConvertToString(ApplicationConstants.MaximumInactivityTimeoutMins));

            DialogEventBus.DialogBoxOk(null, message);
            Log.Debug(message);
            return false;
        }

        public static bool ValidateSignatures(List<SignatureDomain> signatures)
        {
            foreach (var sign in signatures)
            {
                if (!string.IsNullOrEmpty(sign.SignatureMeaning.Trim()) && string.IsNullOrEmpty(sign.SignatureIndicator.Trim()))
                {
                    DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_SignatureBlank"));
                    Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_SignatureBlank"));
                    return false;
                }
            }
            return true;
        }

        public static bool ValidatePasswordExpiryDate(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                DialogEventBus.DialogBoxOk(null, LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordExpiryBlank"));
                Log.Debug(LanguageResourceHelper.Get("LID_ERRMSGBOX_PasswordExpiryBlank"));
                return false;
            }

            if (Convert.ToInt32(value) >= ApplicationConstants.MinimumPasswordExpirationDays &&
                Convert.ToInt32(value) <= ApplicationConstants.MaximumPasswordExpirationDays)
            {
                return true;
            }

            var message = string.Format(LanguageResourceHelper.Get("LID_Label_PassworsExpireLimit"),
                Misc.ConvertToString(ApplicationConstants.MinimumPasswordExpirationDays),
                Misc.ConvertToString(ApplicationConstants.MaximumPasswordExpirationDays));

            DialogEventBus.DialogBoxOk(null, message);
            Log.Debug(message);
            return false;
        }

        #endregion

        public SecuritySettingsDomain SecuritySettingLoad()
        {
            var securityDomain = new SecuritySettingsDomain()
            {
                Signatures = new TrulyObservableCollection<SignatureDomain>(RetrieveSignatureDefinitions()),
                InActivityTimeOutMins = Misc.ConvertToString(GetUserInactivityTimeout()),
                PasswordExpiryDays = Misc.ConvertToString(GetUserPasswordExpiration()),
                SecurityType = SystemStatusFacade.Instance.GetSecurity()
            };

            if (securityDomain.Signatures.Count < 3)
            {
                var count = securityDomain.Signatures.Count;
                for (int i = 1; i <= 3 - count; i++)
                {
                    securityDomain.Signatures.Add(new SignatureDomain());
                }
            }

            return securityDomain;
        }

        public List<SignatureDomain> RetrieveSignatureDefinitions()
        {
            var toReturn = new List<SignatureDomain>();
            var list = ReviewModel.RetrieveSignatureDefinitions();
            list.ForEach(i => toReturn.Add(new SignatureDomain(i.SignatureMeaning, i.SignatureIndicator)));
            return toReturn;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserInactivityTimeout(ushort minutes)
        {
            Log.Debug("SetUserInactivityTimeout:: minutes: " + minutes);
            var hawkeyeError = HawkeyeCoreAPI.User.SetUserInactivityTimeoutAPI(minutes);
            Log.Debug("SetUserInactivityTimeout:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError LogoutUser_Inactivity()
        {
            var hawkeyeError = HawkeyeCoreAPI.User.LogoutUser_InactivityAPI();
            Log.Debug("LogoutUser_Inactivity:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }
    
    
        public ushort GetUserPasswordExpiration()
        {
            ushort days;
            HawkeyeCoreAPI.User.GetUserPasswordExpirationAPI(out days);
            Log.Debug("GetUserPasswordExpiration:: days: " + days);
            return days;
        }
     
        public ushort GetUserInactivityTimeout()
        {
            ushort inActiveTimeOut;
            HawkeyeCoreAPI.User.GetUserInactivityTimeoutAPI(out inActiveTimeOut);
            return inActiveTimeOut;
        }

    
        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError AddSignatureDefinition(SignatureDomain sign)
        {
            Log.Debug("AddSignatureDefinition:: long_text: " + sign.SignatureMeaning + ", short_text: " + sign.SignatureIndicator);
            var hawkeyeError = HawkeyeCoreAPI.Signature.AddSignatureDefinitionAPI(sign);
            Log.Debug("AddSignatureDefinition:: hawkeyeError : " + hawkeyeError);
            return hawkeyeError;
        }


        [MustUseReturnValue("Use HawkeyeError")]
        public HawkeyeError RemoveSignatureDefinition(string signature_short_text, ushort short_text_len)
        {
            var hawkeyeError = HawkeyeCoreAPI.Signature.RemoveSignatureDefinitionAPI(signature_short_text, short_text_len);
            Log.Debug("RemoveSignatureDefinition:: hawkeyeError : " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserPasswordExpiration(UInt16 days)
        {
            Log.Debug("SetUserPasswordExpiration:: days: " + days);
            var hawkeyeError = HawkeyeCoreAPI.User.SetUserPasswordExpirationAPI(days);
            Log.Debug("SetUserPasswordExpiration:: hawkeyeError : " + hawkeyeError);
            return hawkeyeError;
        }

        #endregion
    }
}
