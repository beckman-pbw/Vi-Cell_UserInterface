using ScoutUtilities.Enums;
using System;
using System.Globalization;
using ScoutLanguageResources;

namespace ScoutUI.Common.Converters
{
    public class SeqNamingTypeConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SequentialNamingType seq)
            {
                switch (seq)
                {
                    case SequentialNamingType.Text:
                        return LanguageResourceHelper.Get("LID_Label_Text");
                    case SequentialNamingType.Integer:
                        return LanguageResourceHelper.Get("LID_Label_IncrementingInt");
                }
            }

            return string.Empty;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                if (str.Equals(LanguageResourceHelper.Get("LID_Label_Text"))) return SequentialNamingType.Text;
                if (str.Equals(LanguageResourceHelper.Get("LID_Label_IncrementingInt"))) return SequentialNamingType.Integer;
            }

            return SequentialNamingType.Text;
        }
    }
}