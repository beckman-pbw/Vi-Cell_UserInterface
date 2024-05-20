using System;
using System.Globalization;
using ScoutLanguageResources;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Converters
{
    public class EnumToDisplayWashValueConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var wash = (SamplePostWash)value;
            switch (wash)
            {
                case SamplePostWash.NormalWash:
//                    return LanguageResourceHelper.Get("LID_Label_NormalWorkflowWithUnits");
                    return LanguageResourceHelper.Get("LID_Label_FluidicType_1");
                case SamplePostWash.FastWash:
//                    return LanguageResourceHelper.Get("LID_Label_LCDWorkflowWithUnits");
                    return LanguageResourceHelper.Get("LID_Label_FluidicType_2");
                default:
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

//            if (LanguageResourceHelper.Get("LID_Label_NormalWorkflowWithUnits") == value.ToString())
            if (LanguageResourceHelper.Get("LID_Label_FluidicType_1") == value.ToString())
            {
                return SamplePostWash.NormalWash;
            }
            //            if(LanguageResourceHelper.Get("LID_Label_LCDWorkflowWithUnits") == value.ToString())
            if (LanguageResourceHelper.Get("LID_Label_FluidicType_2") == value.ToString())
            {
                return SamplePostWash.FastWash;
            }
            return string.Empty;
        }
    }
}
