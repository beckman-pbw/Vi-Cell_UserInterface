using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BAFW;

using ScoutUtilities.Delegate;
using ScoutUtilities.Enums;
using ScoutUtilities.Structs;

namespace ExportManager
{
    // **********************************************************************
    public class EvDeleteSamplesReq : BPublicEvent
    {
        public EvDeleteSamplesReq(
            string username,
            string password,
            List<uuidDLL> sampleIds,
            bool keepFirstImage,
            delete_sample_record_callback_pcnt progressCB)
            : base((uint)PubEvIds.SampleDataMgr_Delete)
        {
            Username = username;
            Password = password;            
            KeepFirstImage = keepFirstImage;
            ProgressCB = progressCB;
            foreach(var id  in sampleIds)
            {
                if (id.IsNullOrEmpty())
                {
                    EvAppLogReq.Publish("EvDeleteSamplesReq", EvAppLogReq.LogLevel.Warning, "Removed bad ID", 0);
                }
                else
                {
                    SampleIds.Add(id);
                }
            }
            if(SampleIds.Count == 0)
            {
                EvAppLogReq.Publish("EvDeleteSamplesReq", EvAppLogReq.LogLevel.Error, "No valid ids", 0);
            }
        }

        ~EvDeleteSamplesReq()
        {
        }

        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public List<uuidDLL> SampleIds { get; set; } = new List<uuidDLL>();
        public bool KeepFirstImage { get; set; } = false;
        public delete_sample_record_callback_pcnt ProgressCB { get; set; } = null;

        public static void Publish(
            string username,
            string password,
            List<uuidDLL> sampleIds,
            bool keepFirstImage,
            delete_sample_record_callback_pcnt progressCB
            )
        {
            BAppFW.Publish(new EvDeleteSamplesReq(username, password, sampleIds, keepFirstImage, progressCB));
        }

    }

}
