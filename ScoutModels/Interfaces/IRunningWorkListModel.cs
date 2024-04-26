using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiProxies.Misc;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    public interface IRunningWorkListModel : IDisposable
    {
        event EventHandler<ApiEventArgs<SampleEswDomain>> SampleStatusUpdated;
        event EventHandler<ApiEventArgs<SampleEswDomain>> SampleCompleted;
        event EventHandler<ApiEventArgs<List<uuidDLL>>> WorkListCompleted; //Contains list of SampleDataUuids for the worklist
        event EventHandler<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> ImageResultOccurred;

        event EventHandler<ApiEventArgs<SampleEswDomain>> ConcentrationWorkListItemStatusUpdated;
        event EventHandler<ApiEventArgs<SampleEswDomain>> ConcentrationWorkListItemCompleted;
        event EventHandler<ApiEventArgs<uuidDLL>> ConcentrationWorkListCompleted;
        event EventHandler<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>> ConcentrationWorkListImageResultOccurred;

        bool SetWorkList(WorkListDomain wld, WorkListSource workListSource = WorkListSource.Normal);
        bool StartProcessing(string username, string password);
        bool StopProcessing(string username, string password);
        bool PauseProcessing(string username, string password);
        bool ResumeProcessing(string username, string password);
        Task<bool> AddSampleSetAsync(SampleSetDomain sampleSetDomain);
        Task<bool> CancelSampleSetAsync(ushort sampleSetIndex);
        Task<bool> EjectSampleStageAsync(string username, string password);
    }
}