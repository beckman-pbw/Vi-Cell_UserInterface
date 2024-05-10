using ApiProxies;
using ApiProxies.Commands.QueueManagement;
using ApiProxies.Misc;
using JetBrains.Annotations;
using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Common;
using ScoutModels.ExpandedSampleWorkflow;
using ScoutModels.Interfaces;
using ScoutUtilities.Common;
using ScoutUtilities.Enums;
using ScoutUtilities.Reactive;
using ScoutUtilities.Structs;
using System;
using System.Threading.Tasks;
using System.Windows;
using Ninject.Extensions.Logging;
using System.Collections.Generic;
using ScoutUtilities;
using HawkeyeCoreAPI;

namespace ScoutModels
{
    public class RunningWorkListModel : Disposable, IRunningWorkListModel
    {
        #region Constructor

        public RunningWorkListModel(IWorkListModel workListModel, IScoutModelsFactory scoutModelsFactory, ILogger logger)
        {
            _workListModel = workListModel;
            _scoutModelsFactory = scoutModelsFactory;
            _logger = logger;
            _workListSource = WorkListSource.Normal;
            _worklistSampleDataUuidList = new List<uuidDLL>();
            _eventPump = new AsyncMessagePump<ApiEventArgs>(
                NewThreadSchedulerAccessor.GetNormalPriorityScheduler(nameof(RunningWorkListModel)), 
                RouteNextApiEvent);
            SubscribeToWorkQueue(true);
        }

        protected override void DisposeManaged()
        {
            _eventPump?.Dispose();
            base.DisposeManaged();
        }

        protected override void DisposeUnmanaged()
        {
            SubscribeToWorkQueue(false);
            base.DisposeUnmanaged();
        }

        #endregion

        #region Properties & Fields

        private readonly IWorkListModel _workListModel;
        private readonly IScoutModelsFactory _scoutModelsFactory;
        private readonly ILogger _logger;

        private List<uuidDLL> _worklistSampleDataUuidList;

        private readonly AsyncMessagePump<ApiEventArgs> _eventPump;
        private WorkListSource _workListSource;
        
        public event EventHandler<ApiEventArgs<SampleEswDomain>> SampleStatusUpdated;
        public event EventHandler<ApiEventArgs<SampleEswDomain>> SampleCompleted;
        public event EventHandler<ApiEventArgs<List<uuidDLL>>> WorkListCompleted;
        public event EventHandler<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, 
            ImageSetDto, BasicResultAnswers>> ImageResultOccurred;

        public event EventHandler<ApiEventArgs<SampleEswDomain>> ConcentrationWorkListItemStatusUpdated;
        public event EventHandler<ApiEventArgs<SampleEswDomain>> ConcentrationWorkListItemCompleted;
        public event EventHandler<ApiEventArgs<uuidDLL>> ConcentrationWorkListCompleted;
        public event EventHandler<ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers,
            ImageSetDto, BasicResultAnswers>> ConcentrationWorkListImageResultOccurred;

        #endregion

        #region Public Methods

        public static bool IsMatch(SampleEswDomain sampleEsw, SampleRecordDomain x)
        {
            // We can no longer rely on name matches, so add position and wash:
            return sampleEsw.SampleName.Equals(x.SampleIdentifier) &&
                   sampleEsw.SamplePosition.Equals(x.Position) &&
                   sampleEsw.WashType.Equals(x.WashName);
        }

        public bool SetWorkList(WorkListDomain wld, WorkListSource workListSource = WorkListSource.Normal)
        {
            var result = HawkeyeCoreAPI.WorkList.SetWorkListAPI(wld);

            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to SetWorkListAsync(): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
				Misc.LogOnHawkeyeError("SetWorkListAsync", result);
                _workListSource = workListSource;
            }

            return result == HawkeyeError.eSuccess;
        }

        public bool StartProcessing(string username, string password)
        {
            _workListModel.UpdateStartWorkQueueDateTime(DateTime.Now);
            var result = StartProcessing(username, _workListModel.ImageFilePathGenerator);
            
            if (result != HawkeyeError.eSuccess)
            {
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }

			Misc.LogOnHawkeyeError("StartProcessing", result);

            return result == HawkeyeError.eSuccess;
        }

        public bool StopProcessing(string username, string password)
        {
            var result = HawkeyeCoreAPI.WorkList.StopProcessingAPI(username, password);

            if (result != HawkeyeError.eSuccess)
            {
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }

            Misc.LogOnHawkeyeError("StopProcessing", result);

			return result == HawkeyeError.eSuccess;
        }

        public bool PauseProcessing(string username, string password)
        {
            var result = HawkeyeCoreAPI.WorkList.PauseProcessingAPI(username, password);

            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to PauseProcessingAPI(): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
                _logger.Debug($"PauseProcessingAPI success");
            }

            return result == HawkeyeError.eSuccess;
        }

        public bool ResumeProcessing(string username, string password)
        {
            var result = HawkeyeCoreAPI.WorkList.ResumeProcessingAPI(username, password);

            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to ResumeProcessingAPI(): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
                _logger.Debug($"ResumeProcessingAPI success");
            }

            return result == HawkeyeError.eSuccess;
        }

        public async Task<bool> AddSampleSetAsync(SampleSetDomain sampleSetDomain)
        {
            var result = await _scoutModelsFactory.CreateSecuredTask().Run(() => HawkeyeCoreAPI.WorkList.AddSampleSetAPI(sampleSetDomain));

            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to AddSampleSetAsync(): {result}{Environment.NewLine}{sampleSetDomain.ToString()}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
                _logger.Debug($"Added Sample Set to backend success: {sampleSetDomain.ToString()}");
            }

            return result == HawkeyeError.eSuccess;
        }

        public async Task<bool> CancelSampleSetAsync(ushort sampleSetIndex)
        {
            var result = await _scoutModelsFactory.CreateSecuredTask().Run(() => HawkeyeCoreAPI.WorkList.CancelSampleSetAPI(sampleSetIndex));

            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to CancelSampleSetAPI({sampleSetIndex}): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
                _logger.Debug($"CancelSampleSetAPI success: {sampleSetIndex}");
            }

            return result == HawkeyeError.eSuccess;
        }

        public async Task<bool> EjectSampleStageAsync(string username, string password)
        {
            var result = await _scoutModelsFactory.CreateSecuredTask().Run(() => HawkeyeCoreAPI.Service.EjectSampleStageAPI(username, password));
            if (result != HawkeyeError.eSuccess)
            {
                _logger.Error($"Failed to EjectSampleStageAPI(): {result}");
                ApiHawkeyeMsgHelper.ErrorCommon(result);
            }
            else
            {
                _logger.Debug($"EjectSampleStageAPI success");
            }

            return result == HawkeyeError.eSuccess;
        }

        #endregion

        #region Private Methods

        private void SubscribeToWorkQueue(bool subscribe)
        {
            if (subscribe)
            {
                ApiEventBroker.Instance.Subscribe<SampleEswDomain>(
                    ApiEventType.WorkQueue_Item_Status, HandleWorkListItemStatusUpdate);
                ApiEventBroker.Instance.Subscribe<SampleEswDomain>(
                    ApiEventType.WorkQueue_Item_Completed, HandleWorkListItemCompleted);
                ApiEventBroker.Instance.Subscribe<uuidDLL>(ApiEventType.WorkQueue_Completed,
                    HandleWorkListCompleted);
                ApiEventBroker.Instance.Subscribe<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto,
                    BasicResultAnswers>(ApiEventType.WorkQueue_Image_Result, HandleWorkListImageResult);
            }
            else
            {
                ApiEventBroker.Instance.Unsubscribe<SampleEswDomain>(
                    ApiEventType.WorkQueue_Item_Status, HandleWorkListItemStatusUpdate);
                ApiEventBroker.Instance.Unsubscribe<SampleEswDomain>(
                    ApiEventType.WorkQueue_Item_Completed, HandleWorkListItemCompleted);
                ApiEventBroker.Instance.Unsubscribe<uuidDLL>(ApiEventType.WorkQueue_Completed,
                    HandleWorkListCompleted);
                ApiEventBroker.Instance.Unsubscribe<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>(
                    ApiEventType.WorkQueue_Image_Result, HandleWorkListImageResult);
            }
        }

        private void HandleWorkListImageResult(object sender,
            ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkListCompleted(object sender, ApiEventArgs<uuidDLL> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkListItemCompleted(object sender, ApiEventArgs<SampleEswDomain> e)
        {
            _eventPump.Send(e);
        }

        private void HandleWorkListItemStatusUpdate(object sender, ApiEventArgs<SampleEswDomain> e)
        {
            _eventPump.Send(e);
        }

        private void RouteNextApiEvent(ApiEventArgs args)
        {
            _logger.Error($"RouteNextApiEvent: {args.EventType}");

            switch (args.EventType)
            {
                case ApiEventType.WorkQueue_Completed:
                    var completedArgs = (ApiEventArgs<uuidDLL>)args;
                    switch (_workListSource)
                    {
                        case WorkListSource.Normal:
                            ApiEventArgs<List<uuidDLL>> worklist = new ApiEventArgs<List<uuidDLL>>();
                            worklist.Arg1 = _worklistSampleDataUuidList;
                            WorkListCompleted?.Invoke(this, worklist);
                            _worklistSampleDataUuidList.Clear();
                            break;
                        case WorkListSource.Concentration: ConcentrationWorkListCompleted?.Invoke(this, completedArgs); break;
                    }
                    break;

                case ApiEventType.WorkQueue_Item_Status:
                    var statusArgs = (ApiEventArgs<SampleEswDomain>)args;
                    switch (_workListSource)
                    {
                        case WorkListSource.Normal: SampleStatusUpdated?.Invoke(this, statusArgs); break;
                        case WorkListSource.Concentration: ConcentrationWorkListItemStatusUpdated?.Invoke(this, statusArgs); break;
                    }
                    break;

                case ApiEventType.WorkQueue_Item_Completed:
                    var itemCompleteArgs = (ApiEventArgs<SampleEswDomain>)args;
                    switch (_workListSource)
                    {
                        case WorkListSource.Normal: SampleCompleted?.Invoke(this, itemCompleteArgs);
                            _worklistSampleDataUuidList.Add(itemCompleteArgs.Arg1.SampleDataUuid);
                            break;
                        case WorkListSource.Concentration: ConcentrationWorkListItemCompleted?.Invoke(this, itemCompleteArgs); break;
                    }
                    break;

                case ApiEventType.WorkQueue_Image_Result:
                    var imageArgs = (ApiEventArgs<SampleEswDomain, ushort, BasicResultAnswers, ImageSetDto, BasicResultAnswers>)args;
                    switch (_workListSource)
                    {
                        case WorkListSource.Normal: ImageResultOccurred?.Invoke(this, imageArgs); break;
                        case WorkListSource.Concentration: ConcentrationWorkListImageResultOccurred?.Invoke(this, imageArgs); break;
                    }
                    break;
            }
        }

        [MustUseReturnValue(ApplicationConstants.Justification)]
        private HawkeyeError StartProcessing(string username, Func<Tuple<SampleEswDomain, ushort, BasicResultAnswers,
            ImageSetDto, BasicResultAnswers>, string> filePathGenerator)
        {
            _worklistSampleDataUuidList.Clear();
            // All of the subscriptions will be unhooked when the WorkListCompleted event occurs.
            return new StartProcessing(username, filePathGenerator).Invoke();
        }

        #endregion
    }
}