using ScoutUtilities.Enums;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ScoutUI.Common.Converters
{
    public class ExecutionStatusToBooleanConverter : BaseValueConverter
    {
     
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                var result = (ExecutionStatus) value;
                switch (result)
                {
                    case ExecutionStatus.Default:
                    case ExecutionStatus.Pause:
                    case ExecutionStatus.Skip:
                    case ExecutionStatus.Error:
                        return false;
                    case ExecutionStatus.Running:
                        return true;
                }
            }
            return false;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}