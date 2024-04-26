using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ScoutUI.Common.Converters
{
    public class SubstrateConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is IList<SubstrateType> list)
            {
                return list.Select(GetString).ToList();
            }

            if (value is SubstrateType eSubstrate)
            {
                return GetString(eSubstrate);
            }

            return LanguageResourceHelper.Get("LID_RadioButton_NoneReagent");
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            if (value is IList<string> list)
            {
                return list.Select(GetEnumValue).ToList();
            }

            return GetEnumValue(value.ToString());
        }

        private static string GetString(SubstrateType wash)
        {
            switch (wash)
            {
                case SubstrateType.Carousel: return LanguageResourceHelper.Get("LID_Label_Carousel");
                case SubstrateType.Plate96: return LanguageResourceHelper.Get("LID_Label_96Well");
                case SubstrateType.AutomationCup: return LanguageResourceHelper.Get("LID_Label_AutomationCup");
                default: return LanguageResourceHelper.Get("LID_RadioButton_NoneReagent");
            }
        }

        private static SubstrateType GetEnumValue(string str)
        {
            if (str.Equals(LanguageResourceHelper.Get("LID_Label_Carousel"))) return SubstrateType.Carousel;
            if (str.Equals(LanguageResourceHelper.Get("LID_Label_96Well"))) return SubstrateType.Plate96;
            if (str.Equals(LanguageResourceHelper.Get("LID_Label_AutomationCup"))) return SubstrateType.AutomationCup;
            return SubstrateType.NoType;
        }
    }
}