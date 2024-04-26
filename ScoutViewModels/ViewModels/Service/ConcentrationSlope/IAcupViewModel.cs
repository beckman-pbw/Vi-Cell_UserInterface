using ScoutDomains;
using ScoutDomains.DataTransferObjects;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Enums;
using ScoutUtilities.Helper;
using ScoutUtilities.Structs;
using System.Collections.Generic;

namespace ScoutViewModels.ViewModels.Service.ConcentrationSlope
{
    public interface IHandlesCalibrationState
    {
        /// <summary>
        /// Method called when a new calibration state has been reached.
        /// </summary>
        /// <param name="state"></param>
        void HandleNewCalibrationState(CalibrationGuiState state);
    }

    public interface IHandlesSystemStatus
    {
        /// <summary>
        /// The method called after a new system status occurs.
        /// </summary>
        /// <param name="systemStatus"></param>
        void HandleSystemStatusChanged(SystemStatusDomain systemStatus);
    }

    public interface IHandlesSampleStatus
    {
        /// <summary>
        /// Handler for the sample processing event callback for sample status changed.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="concentration"></param>
        void HandleSampleStatusChanged(SampleEswDomain sample, AcupCalibrationConcentrationListDomain concentration);
    }

    public interface IHandlesSampleCompleted
    {
        /// <summary>
        /// Handler for the sample processing event callback for sample completed.
        /// 
        /// Keep in mind that WorkListComplete may occur ***before*** SampleComplete
        /// due to SampleComplete making a request for SampleResult data.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="sampleResult"></param>
        /// <param name="concentration"></param>
        void HandleSampleCompleted(SampleEswDomain sample, SampleRecordDomain sampleResult,
            AcupCalibrationConcentrationListDomain concentration);
    }

    public interface IHandlesImageReceived
    {
        /// <summary>
        /// Handler for the sample processing event callback for image received.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="imageNum"></param>
        /// <param name="cumulativeResults"></param>
        /// <param name="image"></param>
        /// <param name="imageResults"></param>
        /// <param name="concentration"></param>
        void HandleImageReceived(SampleEswDomain sample, ushort imageNum, BasicResultAnswers cumulativeResults,
            ImageSetDto image, BasicResultAnswers imageResults, AcupCalibrationConcentrationListDomain concentration);
    }

    public interface IHandlesWorkListCompleted
    {
        /// <summary>
        /// Handler for the sample processing event callback for work list completed.
        /// 
        /// Keep in mind that WorkListComplete may occur ***before*** SampleComplete
        /// due to SampleComplete making a request for SampleResult data.
        /// </summary>
        /// <param name="workListUuid"></param>
        void HandleWorkListCompleted(List<uuidDLL> workListUuid);
    }

    public interface IHandlesConcentrationCalculated
    {
        /// <summary>
        /// Handler for updating the ViewModel after concentration has been calculated.
        /// </summary>
        /// <param name="totalCells"></param>
        /// <param name="originalConcentration"></param>
        /// <param name="adjustedConcentration"></param>
        void HandleConcentrationCalculated(
            CalibrationData totalCells,
            CalibrationData originalConcentration,
            CalibrationData adjustedConcentration);
    }
}