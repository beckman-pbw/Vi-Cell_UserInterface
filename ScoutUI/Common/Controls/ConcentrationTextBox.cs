using System;
using System.Windows;
using System.Windows.Controls;
using ScoutDomains;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;

namespace ScoutUI.Common.Controls
{
   
    public class ConcentrationTextBox : Label
    {
        public bool IsConcentrationDataVisible
        {
            get { return (bool) GetValue(IsConcentrationDataVisibleProperty); }
            set { SetValue(IsConcentrationDataVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsConcentrationDataVisibleProperty =
            DependencyProperty.Register("IsConcentrationDataVisible", typeof(bool), typeof(ConcentrationTextBox),
                new PropertyMetadata(false));

        public string ConcentrationDecimalValue
        {
            get { return (string) GetValue(ConcentrationDecimalValueProperty); }
            set { SetValue(ConcentrationDecimalValueProperty, value); }
        }

        public static readonly DependencyProperty ConcentrationDecimalValueProperty =
            DependencyProperty.Register("ConcentrationDecimalValue", typeof(string), typeof(ConcentrationTextBox),
                new PropertyMetadata(string.Empty));

   
        public int ConcentrationNthValue
        {
            get { return (int) GetValue(ConcentrationNthValueProperty); }
            set { SetValue(ConcentrationNthValueProperty, value); }
        }

   
        public static readonly DependencyProperty ConcentrationNthValueProperty =
            DependencyProperty.Register("ConcentrationNthValue", typeof(int), typeof(ConcentrationTextBox),
                new PropertyMetadata(ApplicationConstants.ConcentrationPower));

        public long ConcentrationStartRange
        {
            get { return (long) GetValue(ConcentrationStartRangeProperty); }
            set { SetValue(ConcentrationStartRangeProperty, value); }
        }

        public static readonly DependencyProperty ConcentrationStartRangeProperty =
            DependencyProperty.Register("ConcentrationStartRange", typeof(long), typeof(ConcentrationTextBox),
                new PropertyMetadata(null));


        public long ConcentrationEndRange
        {
            get { return (long) GetValue(ConcentrationEndRangeProperty); }
            set { SetValue(ConcentrationEndRangeProperty, value); }
        }

        public static readonly DependencyProperty ConcentrationEndRangeProperty =
            DependencyProperty.Register("ConcentrationEndRange", typeof(long), typeof(ConcentrationTextBox),
                new PropertyMetadata(null));


        public bool IsConcentrationRangeCheck
        {
            get { return (bool) GetValue(IsConcentrationRangeCheckProperty); }
            set { SetValue(IsConcentrationRangeCheckProperty, value); }
        }

     
        public static readonly DependencyProperty IsConcentrationRangeCheckProperty =
            DependencyProperty.Register("IsConcentrationRangeCheck", typeof(bool), typeof(ConcentrationTextBox),
                new PropertyMetadata(false));

        public bool IsConcentrationForegroundValue
        {
            get { return (bool) GetValue(IsConcentrationForegroundValueProperty); }
            set { SetValue(IsConcentrationForegroundValueProperty, value); }
        }

        public static readonly DependencyProperty IsConcentrationForegroundValueProperty =
            DependencyProperty.Register("IsConcentrationForegroundValue", typeof(bool), typeof(ConcentrationTextBox),
                new PropertyMetadata(false));


        public object ConcentrationDetailsType
        {
            get { return (object)GetValue(ConcentrationDetailsTypeProperty); }
            set { SetValue(ConcentrationDetailsTypeProperty, value); }
        }

        public static readonly DependencyProperty ConcentrationDetailsTypeProperty =
            DependencyProperty.Register("ConcentrationDetailsType", typeof(object), typeof(ConcentrationTextBox), new PropertyMetadata(null));

        public double ConcentrationValue
        {
            get { return (double) GetValue(ConcentrationValueProperty); }
            set { SetValue(ConcentrationValueProperty, value); }
        }

        public static readonly DependencyProperty ConcentrationValueProperty =
            DependencyProperty.Register("ConcentrationValue", typeof(double), typeof(ConcentrationTextBox),
                new PropertyMetadata(Double.NaN, OnConcentrationValueChange));


        private static void OnConcentrationValueChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var concentrationTextBox = d as ConcentrationTextBox;
            if (concentrationTextBox == null)
                return;
            var dilutionValue = 0;
            if (concentrationTextBox.ConcentrationDetailsType is SampleRecordDomain)
            {
                dilutionValue = Convert.ToInt32(((SampleRecordDomain) concentrationTextBox.ConcentrationDetailsType)
                    .DilutionName);
            }
            else if (concentrationTextBox.ConcentrationDetailsType is int)
            {
                dilutionValue = (int) concentrationTextBox.ConcentrationDetailsType;
            }
            else
            {
                dilutionValue = 1;
            }
           
            concentrationTextBox.IsConcentrationDataVisible = !Double.IsNaN(concentrationTextBox.ConcentrationValue);
            concentrationTextBox.ConcentrationDecimalValue =
                ScoutUtilities.Misc.UpdateTrailingPoint(concentrationTextBox.ConcentrationValue / Math.Pow(10, concentrationTextBox.ConcentrationNthValue),
                    ScoutUtilities.Misc.ConcDisplayDigits);

            if (concentrationTextBox.IsConcentrationRangeCheck)
            {
                var concentrationValueAfterDilution = concentrationTextBox.ConcentrationValue / dilutionValue;
                concentrationTextBox.IsConcentrationForegroundValue =
                    concentrationTextBox.ConcentrationStartRange >= concentrationValueAfterDilution ||
                    concentrationTextBox.ConcentrationEndRange <= concentrationValueAfterDilution;
            }
            else
                concentrationTextBox.IsConcentrationForegroundValue = false;
        }
    }
}