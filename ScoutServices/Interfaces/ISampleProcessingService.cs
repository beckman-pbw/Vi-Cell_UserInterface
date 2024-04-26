using ApiProxies.Misc;
using ScoutDataAccessLayer.IDAL;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutServices.Interfaces
{
    public interface ISampleProcessingService
    {
        bool HasPendingDeviceSamples { get; set; }
        SubstrateType LastSubstrate { get; set; }
        bool RunStateChangeInProgress { get; set; }
        WorkListStatus GetWorkListStatus();
        List<KeyValuePair<string, string>> GetParameterList();
        int NumSamplesNotYetRun(IList<SampleSetDomain> sampleSets);

        SampleSetDomain CreateSampleSetFromAutomation(IList<SampleEswDomain> samples,
            string username, string setName, bool usingStartMultipleSamplesMethod,
            out SampleProcessingValidationResult validationResult, ushort setIndex);

        SampleProcessingValidationResult CanProcessSamples(string username, IList<SampleSetDomain> sampleSets, bool carouselIsInstalled);
        bool ProcessSamples(IList<SampleSetDomain> sampleSets, string username,
            SampleSetTemplateDomain sampleSetTemplate, IDataAccess runOptionsSettingsDataAccess);
        Task<bool> AddSampleSetAsync(SampleSetDomain sampleSetDomain);
        Task<bool> CancelSampleSetAsync(ushort setIndex);
        bool StopProcessing(string username, string password);
        bool PauseProcessing(string username, string password);
        bool ResumeProcessing(string username, string password);
        bool EjectStage(string username, string password, bool showDialogPrompt);

        IObservable<ApiEventArgs<SampleEswDomain>> SubscribeToSampleStatusCallback();
        
        /// <summary>
        /// Keep in mind that SampleComplete can be called *after* WorkListComplete due to the
        /// fact that SampleComplete service calls the backend to get the sample results before
        /// being triggered here.
        /// </summary>
        /// <returns></returns>
        IObservable<ApiEventArgs<SampleEswDomain, SampleRecordDomain>> SubscribeToSampleCompleteCallback();
        
        IObservable<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> SubscribeToImageResultCallback();
        
        /// <summary>
        /// Keep in mind that SampleComplete can be called *after* WorkListComplete due to the
        /// fact that SampleComplete service calls the backend to get the sample results before
        /// being triggered here.
        /// </summary>
        /// <returns></returns>
        IObservable<ApiEventArgs<List<uuidDLL>>> SubscribeToWorkListCompleteCallback();
        
        IObservable<WorkListStatus> SubscribeToWorkListStatusChangedCallback();

        void PublishSampleStatusCallback(ApiEventArgs<SampleEswDomain> sampleStatusArgs);
        
        /// <summary>
        /// Keep in mind that SampleComplete can be called *after* WorkListComplete due to the
        /// fact that SampleComplete service calls the backend to get the sample results before
        /// being triggered here.
        /// </summary>
        /// <param name="sampleCompleteArgs"></param>
        void PublishSampleCompleteCallback(ApiEventArgs<SampleEswDomain, SampleRecordDomain> sampleCompleteArgs);
        
        /// <summary>
        /// Keep in mind that SampleComplete can be called *after* WorkListComplete due to the
        /// fact that SampleComplete service calls the backend to get the sample results before
        /// being triggered here.
        /// </summary>
        /// <param name="workListCompleteArgs"></param>
        void PublishWorkListCompleteCallback(ApiEventArgs<List<uuidDLL>> workListCompleteArgs);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageResultArgs">
        ///     SampleEswDomain - The sample that contains the image
        ///     ushort - The image sequence number (EG: 14 out of 100)
        ///     BasicResultAnswers - The cumulative results for sample
        ///     ImageSetDto - Container for the new image
        ///     BasicResultAnswers - The data results for this new image
        /// </param>
        void PublishImageResultCallback(ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> imageResultArgs);
        
        void PublishWorkListStatusChangedCallback(WorkListStatus workListCompleteArgs);
    }
}