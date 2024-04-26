using System;
using System.Globalization;
using System.Windows;
using ScoutLanguageResources;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class NightlyCleanStatusVisibilityConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is eNightlyCleanStatus nightlyCleanStatus)
            {
                if (nightlyCleanStatus == eNightlyCleanStatus.ncsInProgress ||
                    nightlyCleanStatus == eNightlyCleanStatus.ncsAutomationInProgress)
                    return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }
    }

    public class NightCleanInProgressMessage : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is eNightlyCleanStatus nightlyCleanStatus)
            {
                if (nightlyCleanStatus == eNightlyCleanStatus.ncsAutomationInProgress)
                    return LanguageResourceHelper.Get("LID_MSGBOX_AutomationNightCleaning");
                if (nightlyCleanStatus == eNightlyCleanStatus.ncsInProgress)
                    return LanguageResourceHelper.Get("LID_MSGBOX_NightCleaning");
            }

            return string.Empty;
        }
    }
}