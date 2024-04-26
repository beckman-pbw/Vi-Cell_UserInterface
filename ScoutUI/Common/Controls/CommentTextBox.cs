using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Controls
{
    public class CommentTextBox : TextBox
    {
        public bool IsPopOpen
        {
            get { return (bool)GetValue(IsPopOpenProperty); }
            set { SetValue(IsPopOpenProperty, value); }
        }

        public static readonly DependencyProperty IsPopOpenProperty =
            DependencyProperty.Register("IsPopOpen", typeof(bool), typeof(CommentTextBox), new PropertyMetadata(false));

        public string Comment
        {
            get { return (string)GetValue(CommentProperty); }
            set { SetValue(CommentProperty, value); }
        }

        public static readonly DependencyProperty CommentProperty =
            DependencyProperty.Register("Comment", typeof(string), typeof(CommentTextBox), new PropertyMetadata(string.Empty));

        protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs eventArgs)
        {
            IsPopOpen = Boolean.Parse(eventArgs.NewValue.ToString());
        }
    }
}
