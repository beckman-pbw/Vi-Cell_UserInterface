using System.Windows;
using System.Windows.Controls;

namespace ScoutUtilities.Helper
{
    public class PasswordHandler
    {

        public static readonly DependencyProperty PasswordProperty = DependencyProperty.RegisterAttached("Password",
            typeof(string), typeof(PasswordHandler),
            new FrameworkPropertyMetadata(string.Empty, OnPasswordPropertyChanged));

      
        public static readonly DependencyProperty AttachProperty = DependencyProperty.RegisterAttached("Attach",
            typeof(bool), typeof(PasswordHandler),
            new PropertyMetadata(false, Attach));

       
        private static readonly DependencyProperty IsUpdatingProperty = DependencyProperty.RegisterAttached(
            "IsUpdating", typeof(bool),
            typeof(PasswordHandler));


        public static void SetAttach(DependencyObject dp, bool value)
        {
            dp.SetValue(AttachProperty, value);
        }

     
        public static bool GetAttach(DependencyObject dp)
        {
            return (bool)dp.GetValue(AttachProperty);
        }

       
        public static string GetPassword(DependencyObject dp)
        {
            return (string)dp.GetValue(PasswordProperty);
        }

      
        public static void SetPassword(DependencyObject dp, string value)
        {
            dp.SetValue(PasswordProperty, value);
        }

     
        private static bool GetIsUpdating(DependencyObject dp)
        {
            return (bool)dp.GetValue(IsUpdatingProperty);
        }

      
        private static void SetIsUpdating(DependencyObject dp, bool value)
        {
            dp.SetValue(IsUpdatingProperty, value);
        }

      
        private static void OnPasswordPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox != null)
            {
                passwordBox.PasswordChanged -= PasswordChanged;

                if (!GetIsUpdating(passwordBox))
                {
                    passwordBox.Password = e.NewValue == null ? string.Empty : (string)e.NewValue;
                }

                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

     
        private static void Attach(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            if (passwordBox == null)
                return;
            passwordBox.TouchEnter += PasswordBox_TouchEnter;

            if ((bool)e.OldValue)
            {
                passwordBox.PasswordChanged -= PasswordChanged;
            }

            if ((bool)e.NewValue)
            {
                passwordBox.PasswordChanged += PasswordChanged;
            }
        }

       
        private static void PasswordBox_TouchEnter(object sender, System.Windows.Input.TouchEventArgs e)
        {
        }

      
        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var passwordBox = sender as PasswordBox;
            SetIsUpdating(passwordBox, true);
            if (passwordBox == null)
                return;
            SetPassword(passwordBox, passwordBox.Password);
            SetIsUpdating(passwordBox, false);
        }
    }
}