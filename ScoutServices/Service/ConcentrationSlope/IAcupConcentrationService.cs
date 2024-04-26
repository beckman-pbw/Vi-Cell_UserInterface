using ScoutDomains;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Helper;
using System.Collections.Generic;

namespace ScoutServices.Service.ConcentrationSlope
{
    public interface IAcupConcentrationService
    {
        /// <summary>
        /// Gets the full 18 sample definitions to be used in an A cup concentration calibration.
        /// </summary>
        /// <returns></returns>
        List<AcupCalibrationConcentrationListDomain> GetDefaultACupConcentrationList();
        
        /// <summary>
        /// Gets the SampleSetDomain for a single sample to be used in an A cup concentration calibration.
        /// </summary>
        /// <param name="aCupConcentrationComment"></param>
        /// <param name="aCupConcDomain"></param>
        /// <returns></returns>
        SampleSetDomain GetACupConcentrationSampleSetDomain(string aCupConcentrationComment, 
            AcupCalibrationConcentrationListDomain aCupConcDomain);

        /// <summary>
        /// Updates aCupConcentrationList to use the appropriate values from the concentrationTemplates.
        /// This method will assign the user-entered fields into the aCupConcentrationList objects before
        /// sending them to the backend for sample processing.
        /// </summary>
        /// <param name="concentrationTemplates"></param>
        /// <param name="aCupConcentrationList"></param>
        void UpdateACupConcentrationList(IList<ICalibrationConcentrationListDomain> concentrationTemplates,
            IList<AcupCalibrationConcentrationListDomain> aCupConcentrationList);
        
        void CalculateAcupConcentration(IList<SampleRecordDomain> sampleRecords,
            IList<AcupCalibrationConcentrationListDomain> concentrationSamples,
            out CalibrationData totalCells,
            out CalibrationData originalConcentration,
            out CalibrationData adjustedConcentration);
    }
}