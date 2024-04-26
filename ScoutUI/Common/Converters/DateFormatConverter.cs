using ScoutUtilities;
using System;
using System.Globalization;
using ScoutLanguageResources;

namespace ScoutUI.Common.Converters
{
    public class DateFormatConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (!(value is DateTime dt)) return null;

                if (!(parameter is string strParameter)) return Misc.ConvertToString(dt);
                if (strParameter.Equals("Min_NotRun"))
                {
                    if ((dt.Equals(DateTime.MinValue) || dt.Equals(DateTime.MaxValue)))
                    {
                        // if the parameter is "Min_NotRun" and the date hasn't been set, then return "Not run"
                        return LanguageResourceHelper.Get("LID_Status_NotRun");
                    }
                    return Misc.ConvertToCustomShortDateTimeFormat(dt);
                }

                var paramStr = parameter.ToString();
                if (paramStr.Equals("LongDate")) return Misc.ConvertToCustomLongDateTimeFormat(dt);
                if (paramStr.Equals("ShortDate")) return Misc.ConvertToCustomShortDateTimeFormat(dt);
                if (paramStr.Equals("DateOnly")) return Misc.ConvertToCustomDateOnlyFormat(dt);
                return Misc.ConvertToCustomDateFormat(dt, paramStr);
            }
            catch (Exception ex)
            {
                Log.Error("Invalid date time format", ex);
                return null;
            }
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime.TryParse(value?.ToString(), out var dt);
            return dt;
        }
    }
}