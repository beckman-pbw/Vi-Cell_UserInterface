using ScoutDomains;
using System.Collections.Generic;
using System.Linq;
using ScoutDomains.Analysis;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutDomains.RunResult;
using ScoutModels.Review;
using ScoutUtilities;
using ScoutUtilities.Structs;

namespace ScoutModels.Service.ConcentrationSlope
{
    public class AcupConcentrationSlopeModel
    {
        /// <summary>
        /// Maintains the state variables for AcupConcentrationSlopeViewModel.
        /// </summary>
        public AcupConcentrationSlopeModel()
        {
            Reset();
        }

        #region Properties & Fields

        /// <summary>
        /// The list of SampleRecords for an a-cup concentration slope.
        /// This list will likely get updated during the sample processing
        /// image result callback.
        /// </summary>
        public List<SampleRecordDomain> SampleRecordDomains { get; set; }

        /// <summary>
        /// Used to keep track of the current A cup sample being processed.
        /// </summary>
        public int CalibrationSampleIndex { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the model to it's original state.
        /// </summary>
        public void Reset()
        {
            SampleRecordDomains = new List<SampleRecordDomain>();
            CalibrationSampleIndex = 0;
        }

        /// <summary>
        /// Returns the first SampleRecord in SampleRecordDomains that matches
        /// the input SampleEswDomain object.
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public SampleRecordDomain GetSampleRecordDomain(SampleEswDomain sample)
        {
            return SampleRecordDomains.FirstOrDefault(s => s.SampleIdentifier.Equals(sample.SampleName));
        }

        /// <summary>
        /// Updates the SampleRecord within SampleRecordDomains with the new cumulativeResults; or
        /// creates and adds a new SampleRecord to the list if it doesn't exist.
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="cellType"></param>
        /// <param name="cumulativeResults"></param>
        public void UpdateSampleRecord(SampleEswDomain sample, CellTypeDomain cellType, BasicResultAnswers cumulativeResults)
        {
            var sampleRecord = GetSampleRecordDomain(sample);
            if (sampleRecord == null)
            {
                sampleRecord = new SampleRecordDomain
                {
                    Tag = sample.SampleTag,
                    BpQcName = sample.CellTypeQcName,
                    SampleIdentifier = sample.SampleName,
                    WashName = sample.WashType,
                    DilutionName = Misc.ConvertToString(sample.Dilution),
                    Position = sample.SamplePosition,
                    SelectedResultSummary = new ResultSummaryDomain
                    {
                        CellTypeDomain = cellType,
                        CumulativeResult = cumulativeResults.MarshalToBasicResultDomain()
					}
                };

                SampleRecordDomains.Add(sampleRecord);
            }
            else
            {
                sampleRecord.SelectedResultSummary.CumulativeResult = cumulativeResults.MarshalToBasicResultDomain();
            }
        }

        #endregion
    }
}