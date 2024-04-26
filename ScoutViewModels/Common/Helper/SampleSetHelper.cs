using log4net;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutModels.Home.ExpandedSampleWorkflow;
using ScoutUtilities.Structs;
using ScoutViewModels.Interfaces;
using ScoutViewModels.ViewModels.ExpandedSampleWorkflow;
using System;
using System.Threading.Tasks;

namespace ScoutViewModels.Common.Helper
{
    public class SampleSetHelper
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<SampleSetViewModel> GetAsync(uuidDLL sampleSetUuid, IScoutViewModelFactory viewModelFactory)
        {
            try
            {
                SampleSetDomain sampleSetDomain = null;

                await Task.Run(() =>
                {
                    sampleSetDomain = SampleSetModel.Get(sampleSetUuid);
                });

                if (sampleSetDomain == null) return null;

                var ssvm = viewModelFactory.CreateSampleSetViewModel(sampleSetDomain, false, false);
                return ssvm;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to Get SampleSetViewModel from uuid '{sampleSetUuid}'", e);
                return null;
            }
        }
    }
}