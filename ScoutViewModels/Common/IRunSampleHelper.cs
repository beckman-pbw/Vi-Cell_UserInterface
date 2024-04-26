using System;
using System.ComponentModel;
using System.Reactive;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Markup;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Home.QueueManagement;
using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutViewModels.Common
{
    public interface IRunSampleHelper
    {
        bool IsStopActive { get; set; }
        int ImageId { get; set; }
        WorkQueueRecordDomain WorkQueueResultRecord { get; set; }
        SampleRecordDomain SelectedSampleRecord { get; set; }
        bool IsPauseActive { get; set; }
        string Message { get; set; }
        string Title { get; set; }
        QueueManagementModel QueueManagementModel { get; set; }
        start_WorkQueue StartWorkQueue { get; set; }
        bool IsSecurityTurnedOn { get; }
        bool SecurityIsLocal { get; }
        bool IsServiceUser { get; set; }
        bool IsAdminUser { get; set; }
        bool IsAdvancedUser { get; set; }
        bool IsAdminOrServiceUser { get; }

        XmlLanguage CurrentLanguageXml // Used by the DatePicker
        {
            get;
        }

        void AddResultSample(SampleEswDomain wqi, BasicResultAnswers cumulativeResults, 
            BasicResultAnswers imageResults, ImageSetDto image, int imageSequence);

        void UpdateSampleRecord(SampleEswDomain sample, SampleRecordDomain sampleRecord,
            BasicResultAnswers imageResults, ImageSetDto image, int imageSequence);

        void OnUserChanged(UserDomain newUser);
        void DisplayErrorDialogByUser(HawkeyeError result);
        void DisplayErrorDialogByApi(HawkeyeError result, string prefixMessage = null);
        void DisplayLiveImageErrorByAPi(HawkeyeError result);
        event PropertyChangedEventHandler PropertyChanged;
        T GetProperty<T>([CallerMemberName] string propertyName = "");
        void SetProperty<T>(T newValue, NotifyType notifyType = NotifyType.Auto, [CallerMemberName] string propertyName = "");
        string GetSessionVariableAttributeKey(PropertyInfo propertyInfo);
        void NotifyAllPropertiesChanged(bool includeRelayCommands = true);
    }
}