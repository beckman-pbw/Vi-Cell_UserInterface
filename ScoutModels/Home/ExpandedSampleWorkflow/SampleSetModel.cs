using log4net;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities.Structs;
using System;
using System.Text.RegularExpressions;
using ScoutUtilities.Enums;
using SampleSet = HawkeyeCoreAPI.SampleSet;

namespace ScoutModels.Home.ExpandedSampleWorkflow
{
    public class SampleSetModel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static SampleSetDomain Get(uuidDLL sampleSetUuid)
        {
            try
            {
                var result = SampleSet.GetSampleSetAndSampleListApiCall(sampleSetUuid, out var sampleSetDomain);
                return result == HawkeyeError.eSuccess ? sampleSetDomain : null;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to retrieve/parse {nameof(SampleSetDomain)} from uuid '{sampleSetUuid}'.", e);
                return null;
            }
        }

        public static string CleanSequentialNamingDisplayName(string input)
        {
            var output = string.Empty;

            if (!string.IsNullOrEmpty(input))
            {
                output = Regex.Replace(input, @"\[\d+\]", string.Empty);
            }

            return output;
        }
    }
}