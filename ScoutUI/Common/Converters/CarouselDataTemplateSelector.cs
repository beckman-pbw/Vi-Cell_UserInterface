using System.Windows;
using System.Windows.Controls;
using ScoutDomains;

namespace ScoutUI.Common.Converters
{
   
    public class CarouselDataTemplateSelector : DataTemplateSelector
    {
       
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            var sampleDomain = item as BaseSampleDomain;
            if (sampleDomain == null) return null;
            switch (sampleDomain.Type)
            {
                case 1: return element?.FindResource("plateLabelDataTemplate") as DataTemplate;
                case 2: return element?.FindResource("plateButtonDataTemplate") as DataTemplate;
                default: return element?.FindResource("plateEmptyDataTemplate") as DataTemplate;
            }
        }
    }
}