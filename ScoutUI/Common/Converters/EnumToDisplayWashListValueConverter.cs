using ScoutLanguageResources;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace ScoutUI.Common.Converters
{
    public class EnumToDisplayWashListValueConverter : BaseValueConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            var displayWashList = new List<string>();
            var washList = (IList<SamplePostWash>)value;
            foreach (var wash in washList)
            {
                switch (wash)
                {
                    case SamplePostWash.NormalWash:
                        displayWashList.Add(LanguageResourceHelper.Get("LID_Label_NormalWorkflowWithUnits"));
                        break;
                    case SamplePostWash.FastWash:
                        displayWashList.Add(LanguageResourceHelper.Get("LID_Label_LCDWorkflowWithUnits"));
                        break;
                    default:
                        return null;
                }
            }
            return displayWashList;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
           throw new NotImplementedException();
        }
    }
}
