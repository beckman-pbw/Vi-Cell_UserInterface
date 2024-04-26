using JetBrains.Annotations;
using log4net;
using ScoutDomains;
using ScoutDomains.Analysis;
using ScoutLanguageResources;
using ScoutModels.Review;
using ScoutUtilities.Enums;
using ScoutUtilities.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScoutModels
{
    public class StorageModel
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ExportInstrumentConfiguration(string username, string password, string filename)
        {
            var hawkeyeError = HawkeyeCoreAPI.Configuration.ExportInstrumentConfigurationAPI(username, password, filename);
            Log.Debug("ExportInstrumentConfiguration:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        [MustUseReturnValue("Use HawkeyeError")]
        public static HawkeyeError ImportInstrumentConfiguration(string username, string password, string filename)
        {
            Log.Debug("ExportInstrumentConfiguration:: filename: " + filename);
            var hawkeyeError = HawkeyeCoreAPI.Configuration.ImportInstrumentConfigurationAPI(username, password, filename);
            Log.Debug("ExportInstrumentConfiguration:: hawkeyeError: " + hawkeyeError);
            return hawkeyeError;
        }

        public static bool SystemHasData()
        {
            return HawkeyeCoreAPI.Configuration.SystemHasDataAPI();
        }

        public static bool CleanExportDetails(string exportPath)
        {
            if (Directory.Exists(exportPath))
            {
                var files = Directory.GetFiles(exportPath);
                var folders = new DirectoryInfo(exportPath);
                if (files.Any() || folders.GetDirectories().Any())
                {
                    Array.ForEach(Directory.GetFiles(exportPath), File.Delete);

                    foreach (DirectoryInfo dir in folders.GetDirectories())
                    {
                        dir.Delete(true);
                    }

                    MessageBus.Default.Publish(LanguageResourceHelper.Get("LID_MSGBOX_FileDeleted"));
                    return true;
                }
                MessageBus.Default.Publish(LanguageResourceHelper.Get("LID_MSGBOX_NoFile"));
                return false;
            }
            MessageBus.Default.Publish(LanguageResourceHelper.Get("LID_MSGBOX_NoDir"));
            return false;
        }

        public static IList<SampleRecordDomain> GetSampleRecords(UserDomain user, ulong startDate, ulong endDate)
        {
            var userId = string.IsNullOrEmpty(user?.UserID) || user.UserID.Equals(LanguageResourceHelper.Get("LID_Label_All")) ? string.Empty : user.UserID;
            var sampleRecordList = ReviewModel.GetFlattenedResultRecordList_wrappedInSampleRecords(startDate, endDate, userId);
            sampleRecordList.ForEach(x => x.SampleHierarchy = GetSampleHierarchyType(x));
            return sampleRecordList.OrderByDescending(s => s.RetrieveDate).ToList();
        }

        //If a sample is reanalyzed, only the original sample can be selected/unselected. 
        //Any reanalyzed samples appear grayed out
        private static SampleHierarchyType GetSampleHierarchyType(SampleRecordDomain sample)
        {
            if (sample.ResultSummaryList.Count > 1)
            {
                if (Equals(sample.ResultSummaryList.First().UUID, sample.SelectedResultSummary.UUID))
                {
                    return SampleHierarchyType.Parent; //original sample
                }

                return SampleHierarchyType.Child; //reanalyzed sample(s)
            }

            return SampleHierarchyType.None;
        }
    }
}
