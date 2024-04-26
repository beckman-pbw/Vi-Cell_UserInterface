using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ScoutViewModels.ViewModels.Dialogs;

namespace ScoutUI.Views.Dialogs
{
    public partial class AddEditUserDialog : Dialog
    {
        public static char[] InvalidChars = new char[] {' ', '"', '/', '\\', '[', ']', ':', ';', '|', '=', ',', '+', '*', '?', '<', '>', '\'', '{', '}' };

        public AddEditUserDialog(AddEditUserDialogViewModel vm) : base(vm)
        {
            InitializeComponent();
        }

        /**
         * Enforce username not containing invalid characters.
         * [UPN](https://docs.microsoft.com/en-us/windows/win32/secauthn/user-name-formats),
         * an email address, implies '@' is not allowed in the username portion as well. However, we are allowing email addresses in the username.
         * [RFC 822](https://tools.ietf.org/html/rfc822) also mentions no spaces.
         * [Rules for Logon Names](https://docs.microsoft.com/en-us/previous-versions/windows/it-pro/windows-2000-server/bb726984(v=technet.10) state that these characters cannot be used: " / \ [ ] : ; | = , + * ? &lt; &gt;
         */
        private static bool IsTextAllowed(string text)
        {
            return -1 == text.IndexOfAny(InvalidChars);
        }

        private void PreviewUsernameTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = ! IsTextAllowed(e.Text);
        }

        private void UsernamePastingHandler(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void OnPreviewUsernameKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}
