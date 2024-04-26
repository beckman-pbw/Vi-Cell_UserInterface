using JetBrains.Annotations;
using log4net;
using ScoutDomains.Analysis;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScoutModels
{
    public class AnalysisModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

//TODO: not currently used...  SAVE...
        public static IList<int> GetUserAnalyses(string username)
        {
            uint number = 0;
            var userAnalysesList = new List<int>();

            Log.Debug("GetUserAnalyses:: userId:" + username);
            var hawkeyeError = HawkeyeCoreAPI.Analysis.GetUserAnalysisIndicesAPI(username, ref number, ref userAnalysesList);
            Log.Debug("GetUserAnalyses:: hawkeyeError: " + hawkeyeError +
                         ", num analyses: " + userAnalysesList.Count.ToString());

            Log.Debug("GetUserAnalyses::");
            StringBuilder s = new StringBuilder();
            userAnalysesList.ForEach(GetUserAnalyses => { s.Append(GetUserAnalyses + ","); });
            Log.Debug("# anlysis index: " + s);
            s.Clear();

            return userAnalysesList;
        }

        public static List<AnalysisDomain> GetAnalysisDomains(bool forDiagnosticPage)
        {
            if (forDiagnosticPage)
            {
                var tempAnalysisDomains = svc_GetTemporaryAnalysisDefinition();
                return tempAnalysisDomains.Any()
                    ? tempAnalysisDomains
                    : GetAllAnalyses();
            }

            return GetAllAnalyses();
        }

        public static List<AnalysisDomain> GetAllAnalyses()
        {
            var analysisDomainList = new List<AnalysisDomain>();
            var hawkeyeError = HawkeyeCoreAPI.Analysis.GetAllAnalysesAPI(ref analysisDomainList);
            Log.Debug("GetAllAnalyses:: hawkeyeError: " + hawkeyeError + ", # analysis List: " + analysisDomainList.Count);
            return analysisDomainList;
        }

        public static List<AnalysisDomain> svc_GetTemporaryAnalysisDefinition()
        {
            var analysisDomainList = new List<AnalysisDomain>();
            var hawkeyeError = HawkeyeCoreAPI.Analysis.GetTemporaryAnalysisDefinitionAPI(ref analysisDomainList);
            Log.Debug("svc_GetTemporaryAnalysisDefinition:: hawkeyeError: " + hawkeyeError);
            return analysisDomainList;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError SetUserAnalyses(string username, List<UInt16> analysisIndexes)
        {
            uint numberOfAnalyses = Convert.ToUInt16(analysisIndexes.Count);
            Log.Debug("SetUserAnalyses:: username: " + username + ", numberOfAnalyses: " + numberOfAnalyses);
            var hawkeyeError = HawkeyeCoreAPI.Analysis.SetUserAnalysesAPI(username, numberOfAnalyses, analysisIndexes);
            Log.Debug("SetUserAnalyses:: hawkeyeError:  " + hawkeyeError);
            return hawkeyeError;
        }
    }
}

