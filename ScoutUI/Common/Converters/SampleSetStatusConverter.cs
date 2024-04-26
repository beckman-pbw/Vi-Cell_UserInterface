using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class SampleSetStatusConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is SampleSetStatus status)) return string.Empty;

            switch (status)
            {
                case SampleSetStatus.NoSampleSetStatus: return LanguageResourceHelper.Get("LID_Report_Label_Unknown");
                case SampleSetStatus.Pending: return LanguageResourceHelper.Get("LID_Enum_Pending");
                case SampleSetStatus.Running: return LanguageResourceHelper.Get("LID_Enum_Running");
                case SampleSetStatus.Paused: return LanguageResourceHelper.Get("LID_Enum_Paused");
                case SampleSetStatus.Complete: return LanguageResourceHelper.Get("LID_Enum_Complete");
                case SampleSetStatus.Cancelled: return LanguageResourceHelper.Get("LID_Status_Cancelled");

                case SampleSetStatus.SampleSetActive: return LanguageResourceHelper.Get("LID_Enum_Pending");
                case SampleSetStatus.SampleSetInProgress: return LanguageResourceHelper.Get("LID_Enum_Running");
                case SampleSetStatus.SampleSetTemplate: return LanguageResourceHelper.Get("LID_Label_SampleTemplate");

                default: return LanguageResourceHelper.Get("LID_Report_Label_Unknown");
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException($"{nameof(SampleSetStatusConverter)}.{nameof(ConvertBack)}");
        }
    }
}