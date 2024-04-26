using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class FilterItemToStringConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<eFilterItem> eFilterItems)
            {
                var list = new List<string>();
                foreach (var filter in eFilterItems)
                {
                    list.Add(GetString(filter));
                }

                return list;
            }

            if (value is eFilterItem eFilterItemValue)
            {
                switch (eFilterItemValue)
                {
                    case eFilterItem.eSample: return LanguageResourceHelper.Get("LID_Label_SampleFilter");
                    case eFilterItem.eSampleSet: return LanguageResourceHelper.Get("LID_Label_SampleSetFilter");
                }
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string strValue)
            {
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_SampleFilter")))
                    return eFilterItem.eSample;
                if (strValue.Equals(LanguageResourceHelper.Get("LID_Label_SampleSetFilter")))
                    return eFilterItem.eSampleSet;
            }

            return null;
        }

        private string GetString(eFilterItem filterItem)
        {
            switch (filterItem)
            {
                case eFilterItem.eSample: return LanguageResourceHelper.Get("LID_Label_SampleFilter");
                case eFilterItem.eSampleSet: return LanguageResourceHelper.Get("LID_Label_SampleSetFilter");
            }

            return string.Empty;
        }
    }
}