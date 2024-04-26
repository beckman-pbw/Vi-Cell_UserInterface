using ScoutViewModels.ViewModels.Reports;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ScoutUI.Common
{
    public class LogDataLayoutTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is LogPanelViewModel vm)
            {
                switch (vm.ReportId)
                {
                    case 1:
                        var dataTemplateAuditLog = new ResourceDictionary();
                        dataTemplateAuditLog.Source = new Uri("pack://application:,,,/ResourceDictionaries/NamedStyles/DataTemplates/AuditLog.xaml");
                        return dataTemplateAuditLog["AuditLog"] as DataTemplate;
                    case 2:
                        var dataTemplateSampleActivityLog = new ResourceDictionary();
                        dataTemplateSampleActivityLog.Source = new Uri("pack://application:,,,/ResourceDictionaries/NamedStyles/DataTemplates/SampleActivityLog.xaml");
                        return dataTemplateSampleActivityLog["SampleActivityLog"] as DataTemplate;
                    case 3:
                        var dataTemplateSystemErrorLog = new ResourceDictionary();
                        dataTemplateSystemErrorLog.Source = new Uri("pack://application:,,,/ResourceDictionaries/NamedStyles/DataTemplates/SystemErrorLog.xaml");
                        return dataTemplateSystemErrorLog["SystemErrorLog"] as DataTemplate;
                    case 4:
                        var dataTemplateCalibrationHistoryLog = new ResourceDictionary();
                        dataTemplateCalibrationHistoryLog.Source = new Uri("pack://application:,,,/ResourceDictionaries/NamedStyles/DataTemplates/CalibrationHistoryLog.xaml");
                        return dataTemplateCalibrationHistoryLog["CalibrationHistoryLog"] as DataTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}