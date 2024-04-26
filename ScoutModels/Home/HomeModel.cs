using log4net;
using ScoutDomains.EnhancedSampleWorkflow;
using ScoutUtilities;
using ScoutUtilities.CustomEventArgs;
using ScoutUtilities.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SampleSet = HawkeyeCoreAPI.SampleSet;

namespace ScoutModels
{
    public class HomeModel
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<List<SampleSetDomain>> GetSampleSetsAsync(FilterSampleSetsEventArgs args)
        {
            var sampleSets = new List<SampleSetDomain>();
            
            await Task.Run(() =>
            {
                try
                {
                    SampleSet.GetSampleSetListApiCall(args.FilteringItem,
                        DateTimeConversionHelper.DateTimeToUnixSecondRounded(args.FromDate ?? DateTime.Now),
                        DateTimeConversionHelper.DateTimeToUnixSecondRounded(args.ToDate ?? DateTime.Now),
                        args.User, args.SearchString, args.TagSearchString, args.CellTypeOrQualityControlName,
                        0, 0, out var totalQueryResultCount, out sampleSets);
                }
                catch (Exception e)
                {
                    Log.Error($"Failed to get sample sets from filtering args: {args}", e);
                    sampleSets = new List<SampleSetDomain>();
                }
            });

            return sampleSets;
        }

        public static SampleSetDomain GetSampleSet(uuidDLL sampleSetUuid)
        {
            try 
            {
                SampleSet.GetSampleSetAndSampleListApiCall(sampleSetUuid, out var sampleSetDomain);
                return sampleSetDomain;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to get sample set and samples from uuid: {sampleSetUuid}", e);
                return null;
            }
        }
    }
}