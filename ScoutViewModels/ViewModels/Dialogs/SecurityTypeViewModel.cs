using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels.Common;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;

namespace ScoutViewModels.ViewModels.Dialogs
{
    public class SecurityTypeViewModel : BaseDisposableNotifyPropertyChanged
    {
        public SecurityType SecurityType { get; set; }

        public string Name { get; }

        public bool ShowButton
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public virtual bool IsChecked
        {
            get => GetProperty<bool>();
            set => SetProperty(value);
        }

        public SecurityTypeViewModel(SecurityType securityType, UserDomain user = null)
        {
            SecurityType = securityType;
            switch (securityType)
            {
                case SecurityType.NoSecurity:
                    Name = LanguageResourceHelper.Get("LID_Enum_SecuritySettings_None");
                    break;
                case SecurityType.LocalSecurity:
                    Name = LanguageResourceHelper.Get("LID_Enum_SecuritySettings_LocalUsers");
                    break;
                case SecurityType.ActiveDirectory:
                    Name = LanguageResourceHelper.Get("LID_Enum_SecuritySettings_ActiveDirectory");
                    break;
            }

            ShowButton = false;
        }
    }
}