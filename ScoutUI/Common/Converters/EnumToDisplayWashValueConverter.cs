using System;
using System.Globalization;
using System.Windows.Data;
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
                    return LanguageResourceHelper.Get("LID_Label_NormalWashWithUnits");
                case SamplePostWash.FastWash:
                    return LanguageResourceHelper.Get("LID_Label_FastWashWithUnits");
                default:
                    return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (LanguageResourceHelper.Get("LID_Label_NormalWashWithUnits") == value.ToString())
            {
                return SamplePostWash.NormalWash;
            }
            if(LanguageResourceHelper.Get("LID_Label_FastWashWithUnits") == value.ToString())
            {
                return SamplePostWash.FastWash;
            }
            return string.Empty;
        }
    }
}
