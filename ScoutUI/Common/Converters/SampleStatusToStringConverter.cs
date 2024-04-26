using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class SampleStatusToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SampleStatus sampleStatus)
            {
                switch (sampleStatus)
                {
                    case SampleStatus.NotProcessed: return LanguageResourceHelper.Get("LID_Enum_Pending");
                    case SampleStatus.InProcessAspirating: return LanguageResourceHelper.Get("LID_Status_Aspirating");
                    case SampleStatus.InProcessMixing: return LanguageResourceHelper.Get("LID_Status_Mixing");
                    case SampleStatus.InProcessImageAcquisition: return LanguageResourceHelper.Get("LID_Status_ProcessingImages");
                    case SampleStatus.AcquisitionComplete: return LanguageResourceHelper.Get("LID_Status_AcquisitionComplete");
                    case SampleStatus.Completed: return LanguageResourceHelper.Get("LID_Status_Completed");
                    case SampleStatus.SkipError: return LanguageResourceHelper.Get("LID_Label_Error");
                    case SampleStatus.SkipManual: return LanguageResourceHelper.Get("LID_Status_Skipped");
                    case SampleStatus.InProcessCleaning: return LanguageResourceHelper.Get("LID_Status_Cleaning");
                }
            }
            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return SampleStatus.NotProcessed;
            var str = value?.ToString();
            
            if(str == LanguageResourceHelper.Get("LID_Enum_Pending")) return SampleStatus.NotProcessed;
            if(str == LanguageResourceHelper.Get("LID_Status_Aspirating")) return SampleStatus.InProcessAspirating;
            if(str == LanguageResourceHelper.Get("LID_Status_Mixing")) return SampleStatus.InProcessMixing;
            if(str == LanguageResourceHelper.Get("LID_Status_ProcessingImages")) return SampleStatus.InProcessImageAcquisition;
            if(str == LanguageResourceHelper.Get("LID_Status_AcquisitionComplete")) return SampleStatus.AcquisitionComplete;
            if(str == LanguageResourceHelper.Get("LID_Status_Completed")) return SampleStatus.Completed;
            if(str == LanguageResourceHelper.Get("LID_Label_Error")) return SampleStatus.SkipError;
            if(str == LanguageResourceHelper.Get("LID_Status_Skipped")) return SampleStatus.SkipManual;
            if(str == LanguageResourceHelper.Get("LID_Status_Cleaning")) return SampleStatus.InProcessCleaning;

            return SampleStatus.NotProcessed;
        }
    }
}