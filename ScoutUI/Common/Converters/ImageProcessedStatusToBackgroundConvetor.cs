using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using ScoutUtilities.Structs;

namespace ScoutUI.Common.Converters
{
   
    class ImageProcessedStatusToBackgroundConverter : BaseValueConverter
    {
      
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush(Colors.Transparent);

            if (value != null)
                switch ((E_ERRORCODE) value)
                {
                    case E_ERRORCODE.eInvalidInputPath:
                    case E_ERRORCODE.eValueOutOfRange:
                    case E_ERRORCODE.eZeroInput:
                    case E_ERRORCODE.eNullCellsData:
                    case E_ERRORCODE.eInvalidImage:
                    case E_ERRORCODE.eResultNotAvailable:
                    case E_ERRORCODE.eFileWriteError:
                    case E_ERRORCODE.eZeroOutputData:
                    case E_ERRORCODE.eParameterIsNegative:
                    case E_ERRORCODE.eInvalidParameter:
                    case E_ERRORCODE.eBubbleImage:
                    case E_ERRORCODE.eFLChannelsMissing:
                    case E_ERRORCODE.eInvalidCharacteristics:
                    case E_ERRORCODE.eInvalidAlgorithmMode:
                    case E_ERRORCODE.eMoreThanOneFLImageSupplied:
                    case E_ERRORCODE.eTransformationMatrixMissing:
                    case E_ERRORCODE.eFailure:
                    case E_ERRORCODE.eInvalidBackgroundIntensity:
                        brush = new SolidColorBrush(Colors.Yellow);
                        break;
                }
            return brush;

        }

     
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
