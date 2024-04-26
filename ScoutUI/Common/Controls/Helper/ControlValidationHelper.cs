using ScoutLanguageResources;
using ScoutUtilities.Common;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ScoutUI.Common.Helper
{

    public class ControlValidationHelper
    {
      
        public static string EmojiCategoryFirst { get; } =
            "🎈🎂🎅🎉🎍🎋🎒💌💏💒🎀🎃🎆🎊🎎🎐🎓💍💐👸🎁🎄🎇🎌🎏🎑💋💎💑👹\r\n😄🎁😃😉😄😇😌😏😓😘😝😢😊😁😅😈😍😐😔😚😠😣😞😂😆😋😎😒😖😜😡😤\r\n😥😪😰😳😷😀😗😟😬😴😨😫😱😵☺😑😙😦😮🙁😩😭😲😶☹😕😛😧😯🙂\r\n👀👄👇👊👍👐🙊🙇🙍☝👂👅👈👋👎🙈🙅🙋🙎✊👃👆👉👌👏🙉🙆🙌🙏✋\r\n✌❤💕💘💛💞👥👨👫👮👯👬👩👦💟💜💙💖💓🖐🖖💔💗💚💝👤👧👪👭👰\r\n💙💜💟👦👩👬👯👲👵💁💚💝👧👤👪👭👰👳👶💂💛💞👥👨👫👮👱👴👷💃";

     
        public static string EmojiCategorySecond { get; } =
            "🍕🍗🍚🍝🍠🍣🍦🍩🍬🍯🍔🍘🍛🍞🍡🍤🍧🍪🍭🍰🍖🍙🍜🍟🍢🍥🍨🍫🍮🍱\r\n👺👽💀🎿🏂🏆⚽🏉💄💇👻👾🎽🏀🏃🏈⚾🚴💅💈👼👿🎾🏁🏄🏊🏇🚵💆💉\r\n💊🎢🎥🎨🎫🎮🎱🎴🔭🎷🎠🎣🎦🎩🎬🎯🎲🀄🎵🎸🎡🎤🎧🎪🎭🎰🎳🔬🎶🎹\r\n🎬🎯🎲🀄🎵🎸🎻📷📻♣🎭🎰🎳🔬🎶🎹🎼📹📼♥🎮🎱🎴🔭🎷🎺📯📺♠♦\r\n🍲🍴🍷🍺🍼🍇🍊🍎🍒🍐🍳🍵🍸🍻🍅🍈🍌🍏🍓📝🔪🍶☕🍹🍆🍉🍍🍑🍋📞\r\n📟📢📥📨📫📬📲📶🔦🔩📠📣📦📩📮📭📳📵🔧🔫📡📤📧📪📰📱📴🔥🔨 🔮\r\n🔯👒👕👘👛👞👡💻💾📁🔱👓👖👙👜👟👢💼💿📂👑👔👗👚👝👠👣💽📀📃\r\n📄📇📊📍🔊🔉📐📓📖📙📅📈📎📋🔇🔅📑📔📗📚📆📉📌📏🔈🔆📒📕📘📛\r\n📐📓📖📙📜✉🕐🕓🕖🕙📑📔📗📚☎✏🕑🕔🕗🕚📒📕📘📛✂✒🕒🕕🕘🕛";

      
        public static string EmojiCategoryThird { get; } =
            "\r\n✈🚃🚇🚓🚏🚙🚆🚋🚐🚘🚀🚄🚉🚑🚕🚚🚈🚍🚔🚛🚁🚅🚌🚒🚗🚂🚊🚎🚖🚜\r\n🚝🚠🚤🚥🚨⭕🚪🚭🚹🚼🚞🚡🚣🚦⛔🔰🚫🚲🚺🚮🚟🚢💺🚧🅿🚩🚬🚶🚻🚯\r\n🚰🚷🚾🛁⚡🏡🏥🏨🏫🏮🚱🚸🛀♿♻🏢🏦🏬🏩🏯🚳🚽🚿⚠🏠🏣🏧🏪🏭🏰\r\n♻🏢🏦🏩🏬🏯⚓⛳⛽🛃🏠🏣🏧🏪🏭🏰⛪⛵🏤🛄🏡🏥🏨🏫🏮♨⛲⛺🛂🛅";

     
        public static string EmojiCategoryFour { get; } =
            "⛅🌃🌆🌉🌌🌓🌒🌘🌚🌟🌀🌄🌇🌊☄🌔🌖🌙🌜🌠🌁🌅🌈🌋🌑🌕🌗🌛🌝☀\r\n🌞☔☃✴⭐🐎🐔🐙🐜🐟🌂☂✨❄🐌🐑🐗🐚🐝🐠☁⛄✳❇🐍🐒🐘🐛🐞🐡\r\n🐢🐥🐨🐬🐯🐲🐵🐸🐻🐾🐣🐦🐩🐭🐰🐳🐶🐹🐼😸🐤🐧🐫🐮🐱🐴🐷🐺🐽😹\r\n😺😽🙀🐂🐅🐈🐋🐓🐪🌴😻😾🐀🐃🐆🐉🐏🐕🌰🌵😼😿🐁🐄🐇🐊🐐🐖🌱🌷\r\n🐈🐋🐓🐪🌴🌸🌻🌾🍁🍄🐉🐏🌰🐕🌵🌹🌼🌿🍂🌲🐊🐐🐖🌱🌷🌺🌽🍀🍃🌳\r\n⤵〰💡💤💪💧💭💰💳💶⬛〽💢💥💨💫💮💱💴💷⬜💠💣💦💩💬💯💲💵💸\r\n💹🔛↔↗↩⬅🔀🔄🔴🔷🔙🔜↕↘↪⬆🔁🔲🔵🔸🔚🔝↖↙➡⬇🔂🔳🔶🔹\r\n🆙🈂🈲🈵🈸🔃🔍🔐🔓🔖🆚🈚🈳🈶🈹🔋🔎🔑🔔🔗🈁🈯🈴🈷🈺🔌🔏🔒🔕🔘\r\n🔠🔣🔻⌚⏪⏰♉♌♏♒🔡🔤🔼⌛⏫⏳♊♍♐♓🔢🔺🔽⏩⏬♈♋♎♑⛎";


        public static string EmojiCategoryFive { get; } =
            "⁉❔☑❌➖©🌏🌐🗽➰‼❕✅❎✖®🌍🗻🗾➿❓❗✔➕➗™🌎🗼🗿⤴\r\n▪◀◽ℹ⚪🉑🅰🆎🆓🆖▫◻◾🔞⚫㊗🅱🆑🆔🆗▶◼🔟Ⓜ🉐㊙🅾🆒🆕🆘";


        public static string EmojiCategorySix { get; } =
            ";(++¬¬;]:-]:O):|:O(8-)B-)TTOO;;^+^.^:-3-.-**^^;==;[:O=.=;O)=)=D><=[:-*-0-";


        public static string EmojiCategorySeven { get; } =
            ":O):|:O(8-)B-):S>.<:];D:\'(:-3-.-*_*^^;=_=$_$-_-^^^_-Y.Y=D>_<=[:-*-0-:-$> <=]=P=(";

      
        public static string EmojiCategoryEight { get; } = ";):):D:P:(>:(:-O ~_~<3:/";

        
        public static string EmojiCategoryNine { get; } = "^_~^_^^0^:-PU_U>\"<O.O^o^^3^X_X";

      
        public static string EmojiCategoryTen { get; } = ";-):-):-D;P:[)::-():-S:-x=/";

      
        public static bool GetAllowOnlyNumeric(DependencyObject obj)
        {
            return (bool) obj.GetValue(AllowOnlyNumericProperty);
        }

      
        public static void SetAllowOnlyNumeric(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowOnlyNumericProperty, value);
        }


        public static bool GetAssayValueValidate(DependencyObject obj)
        {
            return (bool) obj.GetValue(AssayValueValidateProperty);
        }

        public static void SetAssayValueValidate(DependencyObject obj, bool value)
        {
            obj.SetValue(AssayValueValidateProperty, value);
        }

        public static readonly DependencyProperty AssayValueValidateProperty =
            DependencyProperty.RegisterAttached("AssayValueValidate", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, ValidateAssayValue));

        private static void ValidateAssayValue(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox) dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    char decimalSeparator =
                        Convert.ToChar(LanguageResourceHelper.CurrentFormatCulture.NumberFormat.NumberDecimalSeparator);
                    var multipleCount = textBox.Text.Split(decimalSeparator);
                    if (!Regex.IsMatch(textBox.Text, $"[^{decimalSeparator}0-9]") && multipleCount.Length <= 2)
                        return;
                    var s = textBox.Text;
                    s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                    textBox.Text = s;
                    textBox.SelectionStart = s.Length;
                }
            };
        }


        public static bool GetDateTimeValidate(DependencyObject obj)
        {
            return (bool) obj.GetValue(DateTimeValidateProperty);
        }

        public static void SetDateTimeValidate(DependencyObject obj, bool value)
        {
            obj.SetValue(DateTimeValidateProperty, value);
        }

        public static readonly DependencyProperty DateTimeValidateProperty =
            DependencyProperty.RegisterAttached("DateTimeValidate", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, ValidateDateTime));

        private static void ValidateDateTime(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is DatePicker))
                return;
            var dateTimePicker = (DatePicker) dp;
            dateTimePicker.PreviewKeyDown += (sender, arg) =>
            {
                if (arg.Key < Key.D0 || arg.Key > Key.D9)
                {
                    if (arg.Key < Key.NumPad0 || arg.Key > Key.NumPad9)
                    {
                        if (arg.Key != Key.Back && arg.Key != Key.Oem2 && arg.Key != Key.System)
                        {
                            arg.Handled = true;
                        }

                        if (arg.Key == Key.Delete || arg.Key == Key.Back)
                        {
                            dateTimePicker.Text = "";
                            dateTimePicker.SelectedDate = null;
                        }
                    }
                }
            };
            dateTimePicker.LostFocus += (sender, arg) =>
            {
                if (dateTimePicker.Text == "")
                {
                    dateTimePicker.SelectedDate = DateTime.Now;
                }

                DateTime datetimeText;
                DateTime.TryParse(dateTimePicker.Text, out datetimeText);
                if (!(datetimeText >= DateTime.Now))
                {
                    dateTimePicker.SelectedDate = DateTime.Now;
                }
            };
        }

        public static bool GetAllowOnlyDouble(DependencyObject obj)
        {
            return (bool) obj.GetValue(AllowOnlyDoubleProperty);
        }

        public static void SetAllowOnlyDouble(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowOnlyDoubleProperty, value);
        }

        public static readonly DependencyProperty AllowOnlyDoubleProperty =
            DependencyProperty.RegisterAttached("AllowOnlyDouble", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, AllowOnlyDouble));


        public static readonly DependencyProperty AllowOnlyNumericProperty =
            DependencyProperty.RegisterAttached("AllowOnlyNumeric", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, AllowOnlyNumeric));


        public static bool GetAllowOnlyAssayValue(DependencyObject obj)
        {
            return (bool) obj.GetValue(AllowOnlyAssayValueProperty);
        }

        public static void SetAllowOnlyAssayValue(DependencyObject obj, bool value)
        {
            obj.SetValue(AllowOnlyAssayValueProperty, value);
        }

        public static readonly DependencyProperty AllowOnlyAssayValueProperty =
            DependencyProperty.RegisterAttached("AllowOnlyAssayValue", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, AllowOnlyAssayValue));

        private static void AllowOnlyAssayValue(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox) dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox == null)
                    return;

                char decimalSeparator = Convert.ToChar(LanguageResourceHelper.CurrentFormatCulture.NumberFormat.NumberDecimalSeparator);
                Regex objNotPositivePattern = new Regex($"[^0-9{decimalSeparator}]");
                Regex objPositivePattern = new Regex($"^[{decimalSeparator}][0-9]+$|[0-9]*[{decimalSeparator}]*[0-9]+$");
                Regex objTwoDotPattern = new Regex($"[0-9]*[{decimalSeparator}][0-9]*[{decimalSeparator}][0-9]*");

                if (!objNotPositivePattern.IsMatch(textBox.Text) && objPositivePattern.IsMatch(textBox.Text) &&
                    !objTwoDotPattern.IsMatch(textBox.Text))
                    return;

                var s = textBox.Text;
                s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                textBox.Text = s;
                textBox.SelectionStart = s.Length;
            };
        }


        public static bool GetControlScroll(DependencyObject obj)
        {
            return (bool) obj.GetValue(ControlScrollProperty);
        }

     
        public static void SetControlScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(ControlScrollProperty, value);
        }

    
        public static readonly DependencyProperty ControlScrollProperty =
            DependencyProperty.RegisterAttached("ControlScroll", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, ControlScrollCallBack));


        private static void AllowOnlyNumeric(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox) dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox == null || !Regex.IsMatch(textBox.Text, "[^0-9]"))
                    return;
                var s = textBox.Text;
                s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                textBox.Text = s;
                textBox.SelectionStart = s.Length;
            };
        }


        private static void AllowOnlyDouble(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox)) return;

            var txtObj = (TextBox) dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                if (sender is TextBox textBox)
                {
                    char decimalSeparator =  Convert.ToChar(LanguageResourceHelper.CurrentFormatCulture.NumberFormat.NumberDecimalSeparator);
                    var multipleCount = textBox.Text.Split(decimalSeparator);
                    if (textBox.Text.StartsWith(decimalSeparator.ToString()))
                    {
                        textBox.Text = string.Empty;
                        return;
                    }
                    if (!Regex.IsMatch(textBox.Text, $"[^(-?){decimalSeparator}0-9]") && multipleCount.Length <= 2)
                        return;
                    var s = textBox.Text;
                    s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                    textBox.Text = s;
                    textBox.SelectionStart = s.Length;
                }
            };
        }

      
        private static void ControlScrollCallBack(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (dp.GetType() == typeof(ComboBox))
            {
                var cmbBox = (ComboBox) dp;
                cmbBox.TouchEnter += (sender, arg) =>
                {
                    var comboBox = sender as ComboBox;
                    if (comboBox == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(comboBox, ScrollBarVisibility.Auto);
                    ScrollViewer.SetHorizontalScrollBarVisibility(comboBox, ScrollBarVisibility.Auto);
                };
                cmbBox.TouchLeave += (sender, arg) =>
                {
                    var comboBox = sender as ComboBox;
                    if (comboBox == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(comboBox, ScrollBarVisibility.Hidden);
                    ScrollViewer.SetHorizontalScrollBarVisibility(comboBox, ScrollBarVisibility.Hidden);
                };
            }

            if (dp.GetType() == typeof(ListView))
            {
                var lstView = (ListView) dp;
                lstView.TouchEnter += (sender, arg) =>
                {
                    var listView = sender as ListView;
                    if (listView == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(listView, ScrollBarVisibility.Auto);
                    ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Auto);
                };
                lstView.TouchLeave += (sender, arg) =>
                {
                    var listView = sender as ListView;
                    if (listView == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(listView, ScrollBarVisibility.Hidden);
                    ScrollViewer.SetHorizontalScrollBarVisibility(listView, ScrollBarVisibility.Hidden);
                };
            }

            if (dp.GetType() != typeof(ListBox))
                return;
            {
                var lstBox = (ListBox) dp;
                lstBox.TouchEnter += (sender, arg) =>
                {
                    var listBox = sender as ListBox;
                    if (listBox == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(listBox, ScrollBarVisibility.Auto);
                    ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Auto);
                };
                lstBox.TouchLeave += (sender, arg) =>
                {
                    var listBox = sender as ListBox;
                    if (listBox == null)
                        return;
                    ScrollViewer.SetVerticalScrollBarVisibility(listBox, ScrollBarVisibility.Hidden);
                    ScrollViewer.SetHorizontalScrollBarVisibility(listBox, ScrollBarVisibility.Hidden);
                };
            }
        }

        public static bool GetDisableSmiley(DependencyObject obj)
        {
            return (bool) obj.GetValue(DisableSmileyProperty);
        }
    
        public static void SetDisableSmiley(DependencyObject obj, bool value)
        {
            obj.SetValue(DisableSmileyProperty, value);
        }
   
        public static readonly DependencyProperty DisableSmileyProperty =
            DependencyProperty.RegisterAttached("DisableSmiley", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, DisableSmileyCallBack));

        private static void DisableSmileyCallBack(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox) dp;
            txtObj.PreviewTextInput += (sender, arg) =>
            {
                if (EmojiCategoryFirst.Contains(arg.Text) || EmojiCategorySecond.Contains(arg.Text)
                || EmojiCategoryThird.Contains(arg.Text) || EmojiCategoryFour.Contains(arg.Text)
                || EmojiCategoryFive.Contains(arg.Text) || EmojiCategorySix.Contains(arg.Text)
                || EmojiCategorySeven.Contains(arg.Text) || EmojiCategoryEight.Contains(arg.Text)
                || EmojiCategoryNine.Contains(arg.Text) || EmojiCategoryTen.Contains(arg.Text))
                {
                    arg.Handled = true;
                }
            };
        }

     
        public static bool GetTouchValidate(DependencyObject obj)
        {
            return (bool) obj.GetValue(TouchValidateProperty);
        }

    
        public static void SetTouchValidate(DependencyObject obj, bool value)
        {
            obj.SetValue(TouchValidateProperty, value);
        }

    
        public static readonly DependencyProperty TouchValidateProperty =
            DependencyProperty.RegisterAttached("TouchValidate", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, ValidateTouch));

      
        private static void ValidateTouch(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is FrameworkElement))
                return;
            var control = (FrameworkElement) dp;
            control.TouchEnter += (sender, arg) =>
            {
                var textBlock = control as TextBlock;
                if (textBlock != null)
                {
                    var text = LanguageResourceHelper.Get("LID_UsersLabel_CellType");
                    if(!textBlock.Text.Equals("") && textBlock.Tag.Equals(text))
                    {
                        var toolTip = control.ToolTip as ToolTip;
                        if (toolTip != null)
                        {
                            toolTip.Content = textBlock.Text;
                            ValidateTooltip(toolTip);
                        }
                    }

                    if (textBlock.Tag.Equals(1))
                    {
                        var toolTip = control.ToolTip as ToolTip;
                        if (toolTip != null)
                        {
                            toolTip.Content = textBlock.Text;
                            ValidateTooltip(toolTip);
                        }
                    }
                }
            };
        }

        private static async void ValidateTooltip(ToolTip toolTip)
        {
            if (toolTip == null)
                return;
            toolTip.IsOpen = true;
            toolTip.StaysOpen = true;
            await Task.Delay(ApplicationConstants.DelayTimer);
            toolTip.IsOpen = false;
        }


        public static bool GetTrimText(DependencyObject obj)
        {
            return (bool)obj.GetValue(TrimTextProperty);
        }

        public static void SetTrimText(DependencyObject obj, bool value)
        {
            obj.SetValue(TrimTextProperty, value);
        }

        public static readonly DependencyProperty TrimTextProperty =
            DependencyProperty.RegisterAttached("TrimText", typeof(bool), typeof(ControlValidationHelper), 
                new PropertyMetadata(false, OnTrimText));


        private static void OnTrimText(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox)dp;
            txtObj.LostFocus += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    textBox.Text = textBox.Text.Trim();
                }
            };
        }

        public static bool GetCharacterValidation(DependencyObject obj)
        {
            return (bool)obj.GetValue(CharacterValidationProperty);
        }

        public static void SetCharacterValidation(DependencyObject obj, bool value)
        {
            obj.SetValue(CharacterValidationProperty, value);
        }

        public static readonly DependencyProperty CharacterValidationProperty =
            DependencyProperty.RegisterAttached("CharacterValidation", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, OnValidateCharacter));

        private static void OnValidateCharacter(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox)dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                var invalidChar = Path.GetInvalidFileNameChars();
                string myStr = "";
                if (textBox != null)
                {
                    if (textBox.Text.Length == 0)
                        return;

                    var curIdx = textBox.SelectionStart;
                    var textArray = textBox.Text.ToCharArray();

                    if (!textArray.Any(x => invalidChar.Any(y => y.Equals(x))))
                        return; // No invalid chars - don't do anything

                    // Remove invalid characters
                    for (int j = 0; j < textArray.Count(); j++)
                    {
                        var v = textArray[j];
                        if (!invalidChar.Contains(v))
                            myStr += v;
                        else if (curIdx > 0)
                            curIdx--;
                    }
                    if (curIdx < 0) curIdx = 0;

                    textBox.Text = myStr;
                    textBox.SelectionStart = curIdx;
                }
            };
        }

        public static bool GetSerialNumberValidation(DependencyObject obj)
        {
            return (bool)obj.GetValue(SerialNumberValidationProperty);
        }

        public static void SetSerialNumberValidation(DependencyObject obj, bool value)
        {
            obj.SetValue(SerialNumberValidationProperty, value);
        }

        public static readonly DependencyProperty SerialNumberValidationProperty =
            DependencyProperty.RegisterAttached("SerialNumberValidation", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, OnValidateSerialNumber));

        private static void OnValidateSerialNumber(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox)dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox == null || !Regex.IsMatch(textBox.Text, "[^\\w-]"))
                    return;
                var s = textBox.Text;
                s = s.Length > 1 ? s.Substring(0, s.Length - 1) : string.Empty;
                textBox.Text = s;
                textBox.SelectionStart = s.Length;
            };
        }

        public static bool GetRestrictSpace(DependencyObject obj)
        {
            return (bool)obj.GetValue(RestrictSpaceProperty);
        }

        public static void SetRestrictSpace(DependencyObject obj, bool value)
        {
            obj.SetValue(RestrictSpaceProperty, value);
        }

        public static readonly DependencyProperty RestrictSpaceProperty =
            DependencyProperty.RegisterAttached("RestrictSpace", typeof(bool), typeof(ControlValidationHelper),
                new PropertyMetadata(false, OnRestrictSpace));

        private static void OnRestrictSpace(DependencyObject dp, DependencyPropertyChangedEventArgs e)
        {
            if (!(dp is TextBox))
                return;
            var txtObj = (TextBox)dp;
            txtObj.TextChanged += (sender, arg) =>
            {
                var textBox = sender as TextBox;
                if (textBox != null)
                {
                    if(textBox.Text.Equals(" ") || textBox.Text.Equals(string.Empty))
                    {
                        var text = textBox.Text;
                        text = text.Length > 1 ? text.Substring(0, text.Length - 1) : string.Empty;
                        textBox.Text = text;
                        textBox.SelectionStart = text.Length;
                    }
                }
            };
        }
    }
}