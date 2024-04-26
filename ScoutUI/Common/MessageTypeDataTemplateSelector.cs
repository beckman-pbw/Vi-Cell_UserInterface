using ScoutUtilities.Common;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common
{
    public class MessageTypeDataTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var element = container as FrameworkElement;

            if (element != null && item != null && item is HubMessage)
            {
                var message = item as HubMessage;

                if (message.TimesShown > 0)
                    return element.FindResource("IsReadMessageTemplate") as DataTemplate;
                else
                    return element.FindResource("UnreadMessageTemplate") as DataTemplate;
            }

            return null;
        }
    }
}
