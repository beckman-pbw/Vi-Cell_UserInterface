using ScoutLanguageResources;
using ScoutUtilities;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Views.ucCommon
{
    /// <summary>
    /// Interaction logic for AssayValueTextBox.xaml
    /// </summary>
    public partial class AssayValueTextBox
    {
        public AssayValueTextBox()
        {
            InitializeComponent();
        }

        private void OnAssayValueLostFocus(object sender, RoutedEventArgs e)
        {
            var assayValuetxTextBox = sender as TextBox;
            if (assayValuetxTextBox != null)
                assayValuetxTextBox.Text = Misc.UpdateTrailingPoint(Misc.DoubleTryParse(assayValuetxTextBox.Text) ?? 0.0, TrailingPoint.Two);
        }

        public string AssayValue
        {
            get { return (string) GetValue(AssayValueProperty); }
            set { SetValue(AssayValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssayValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssayValueProperty =
            DependencyProperty.Register("AssayValue", typeof(string), typeof(AssayValueTextBox),
                new PropertyMetadata((s, e) => OnAssayValueChange(s as AssayValueTextBox, e.NewValue, e.OldValue)));

        private static void OnAssayValueChange(AssayValueTextBox assayValueTextBox, object newValue, object oldValue)
        {
            double newValueData = Misc.DoubleTryParse(newValue.ToString()) ?? 0.0;
            switch (assayValueTextBox.AssayValueType)
            {
                case AssayValueEnum.M2:
                    newValueData = newValueData * Math.Pow(10, assayValueTextBox.AssayValuePower);
                    assayValueTextBox.IsAssayValueCorrect = newValueData >= ApplicationConstants.Con2MStartRange &&
                                                            newValueData <= ApplicationConstants.Con2MEndRange;
                    break;
                case AssayValueEnum.M4:
                    newValueData = newValueData * Math.Pow(10, assayValueTextBox.AssayValuePower);
                    assayValueTextBox.IsAssayValueCorrect = newValueData >= ApplicationConstants.Con4MStartRange &&
                                                            newValueData <= ApplicationConstants.Con4MEndRange;
                    break;
                case AssayValueEnum.M10:
                    newValueData = newValueData * Math.Pow(10, assayValueTextBox.AssayValuePower);
                    assayValueTextBox.IsAssayValueCorrect = newValueData >= ApplicationConstants.Con10MStartRange &&
                                                            newValueData <= ApplicationConstants.Con10MEndRange;
                    break;
            }
        }


        public int AssayValuePower
        {
            get { return (int) GetValue(AssayValuePowerProperty); }
            set { SetValue(AssayValuePowerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssayValuePower.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssayValuePowerProperty =
            DependencyProperty.Register("AssayValuePower", typeof(int), typeof(AssayValueTextBox),
                new PropertyMetadata(ApplicationConstants.ConcentrationPower));


        public AssayValueEnum AssayValueType
        {
            get { return (AssayValueEnum) GetValue(AssayValueTypeProperty); }
            set { SetValue(AssayValueTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AssayValueType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AssayValueTypeProperty =
            DependencyProperty.Register("AssayValueType", typeof(AssayValueEnum), typeof(AssayValueTextBox),
                new PropertyMetadata(null));


        public bool IsAssayValueCorrect
        {
            get { return (bool) GetValue(IsAssayValueCorrectProperty); }
            set { SetValue(IsAssayValueCorrectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsAssayValueCorrect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsAssayValueCorrectProperty =
            DependencyProperty.Register("IsAssayValueCorrect", typeof(bool), typeof(AssayValueTextBox),
                new PropertyMetadata(false));


        private void AssayValueTextChange(object sender, TextChangedEventArgs e)
      {
            var textBox = sender as TextBox;

            if (textBox != null)
            {
                char decimalSeparator = Convert.ToChar(LanguageResourceHelper.CurrentFormatCulture.NumberFormat.NumberDecimalSeparator);
                var multipleCount = textBox.Text.Split(decimalSeparator);
                var regex = new Regex($"[^{decimalSeparator}0-9]");
                if (!regex.IsMatch(textBox.Text) && multipleCount.Length <= 2)
                    return;
                var s = textBox.Text;
                s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                textBox.Text = s;
                textBox.SelectionStart = s.Length;
            }
        }

        private void GridTouch_down(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
            return;
        }

        private void GridTouch_down(object sender, System.Windows.Input.TouchEventArgs e)
        {
            e.Handled = true;
            return;
        }
    }
}