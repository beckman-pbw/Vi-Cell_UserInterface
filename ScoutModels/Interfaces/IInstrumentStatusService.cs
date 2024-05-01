using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutUtilities.Structs;

namespace ScoutModels.Interfaces
{
    public interface IInstrumentStatusService 
    {
        IObservable<SystemStatusDomain> SubscribeToSystemStatusCallback();
        void PublishSystemStatusCallback(SystemStatusDomain systemStatus);

        IObservable<string> SubscribeViCellIdentifierCallback();
        void PublishViCellIdentifierCallback(string serialNumber);

        IObservable<string> SubscribeSoftwareVersionCallback();
        void PublishSoftwareVersionCallback(string version);

        IObservable<string> SubscribeFirmwareVersionCallback();
        void PublishFirmwareVersionCallback(string version);

        IObservable <ErrorStatusType> SubscribeErrorStatusCallback();
        void PublishErrorStatusCallback(ErrorStatusType status);

        IObservable<List<CellTypeDomain>> SubscribeToCellTypesCallback();

        IObservable<Int32> SubscribeReagentUseRemainingCallback();
        void PublishReagentUseRemainingCallback(Int32 reagentUseRemaining);

        IObservable<Int32> SubscribeWasteTubeCapacityCallback();
        void PublishWasteTubeCapacityCallback(Int32 wasteTubeCapacity);

        void GetAndPublishSystemStatus();

        SystemStatusDomain SystemStatusDom { get; }

        SystemStatus SystemStatus { get; }

        HawkeyeError SampleTubeDiscardTrayEmptied();

        bool IsRunning();

        /// <summary>
        /// The system is not running. It is either Idle, Stopped, or has Faulted.
        /// </summary>
        /// <returns>true if not running, otherwise false, included stopping.</returns>
        bool IsNotRunning();

        bool IsPaused();
        HawkeyeError ClearSystemErrorCode(uint active_error);
    }
}