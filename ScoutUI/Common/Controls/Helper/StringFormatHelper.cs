using ScoutUtilities;
using ScoutUtilities.Enums;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls.Helper
{
    public class StringFormatHelper
    {
        public static bool GetConcentrationFormat(DependencyObject obj)
        {
            return (bool) obj.GetValue(ConcentrationFormatProperty);
        }

        public static void SetConcentrationFormat(DependencyObject obj, bool value)
        {
            obj.SetValue(ConcentrationFormatProperty, value);
        }

        public static readonly DependencyProperty ConcentrationFormatProperty =
            DependencyProperty.RegisterAttached("ConcentrationFormat", typeof(bool), typeof(StringFormatHelper),
                new PropertyMetadata(false, OnConcentrationFormatChange));

        private static void OnConcentrationFormatChange(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox) dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    var concentration = Misc.DoubleTryParse(textBox.Text) ?? 0.0;
                    double tempConcentration = concentration;
                    var count = 0;
                    while (concentration > 10)
                    {
                        concentration = concentration / 10;
                        count++;
                    }

                    if (count > 0)
                        textBox.Text = Misc.UpdateTrailingPoint(tempConcentration / Math.Pow(10, count), TrailingPoint.Two) + "10^" + count;
                    else
                        textBox.Text = Misc.UpdateTrailingPoint(tempConcentration / Math.Pow(10, count), TrailingPoint.Two);
                }
            };
        }
    }
}