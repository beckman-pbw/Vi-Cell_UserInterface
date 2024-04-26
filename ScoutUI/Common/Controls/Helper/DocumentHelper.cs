using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common.Helper
{
   
    public static class DocumentHelper
    {
       
        public static readonly DependencyProperty BindableSourceProperty =
            DependencyProperty.RegisterAttached("BindableSource", typeof(string), typeof(DocumentHelper),
                new UIPropertyMetadata(null, BindableSourcePropertyChanged));

     
        public static string GetBindableSource(DependencyObject obj)
        {
            return (string) obj.GetValue(BindableSourceProperty);
        }

     
        public static void SetBindableSource(DependencyObject obj, string value)
        {
            obj.SetValue(BindableSourceProperty, value);
        }

      
        public static void BindableSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var browser = o as WebBrowser;
            if (browser == null)
                return;
            string uri = "file:\\" + AppDomain.CurrentDomain.BaseDirectory + "HelpDocuments\\Illustrato_file-3.pdf";
            browser.Navigate(uri);
        }
    }
}