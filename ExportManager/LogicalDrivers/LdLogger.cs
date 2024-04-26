using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

using BAFW;

namespace ExportManager
{

    // **********************************************************************
    public class LdLogger : BLogicalDriver
    {

        private string _filename = "";
        // **********************************************************************
        public LdLogger(string filename)
        {
            _filename = filename;
            StartThread("LdLogger");
            _tmrPrune = new BTimer(GetEventQueue(), (uint)TimerIds.Prune);
            _tmrPrune.FireInSecs(kPRUNE_SECONDS);
        }

        // **********************************************************************
        ~LdLogger()
        {
            Shutdown();
        }

        private BTimer _tmrPrune;
        // ***********************************************************************
        private enum TimerIds
        {
            Prune = 0,
        }


        // **********************************************************************
        public void AddEntry(string logstr)
        {
            AddEntryEv logEv = new AddEntryEv(logstr);
            PostEvent(logEv);
        }

        // **********************************************************************
        public class AddEntryEv : BPrivateEvent
        {
            public AddEntryEv(string logStr) : base((uint)PrivateEvIds.AddEntry)
            {
                LogStr = logStr;
            }
            public string LogStr { get; set; } = "";
        }

        // **********************************************************************
        private enum PrivateEvIds
        {
            AddEntry = 0,
        }

        private const int kMAX_ENTRIES = 12000;
        private const int kPRUNE_BUFF = 750;
        private const int kPRUNE_SECONDS = 179;
        // **********************************************************************
        protected override void ProcessEvent(BEvent ev)
        {
            switch (ev.MyType)
            {
                //...................................................................
                case BEvent.EvType.Private:
                {
                    if (ev.Id == (uint)PrivateEvIds.AddEntry)
                    {
                        AddEntryEv logEv = (AddEntryEv)ev;
                        File.AppendAllText(_filename, logEv.LogStr + "\r\n");
                        return;
                    }
                    break;
                }
                //...................................................................
                case BEvent.EvType.Timer:
                {
                    try
                    {
                        if (File.Exists(_filename))
                        {
                            var allLines = File.ReadAllLines(_filename);
                            int count = allLines.Count();
                            if (count > kMAX_ENTRIES)
                            {
                                List<string> lineList = allLines.ToList();
                                lineList.RemoveRange(0, (count - kMAX_ENTRIES));
                                lineList.RemoveRange(0, kPRUNE_BUFF);
                                lineList.Insert(0, "Log Pruned: " + DateTime.Now.ToLongTimeString());
                                File.WriteAllLines(_filename, lineList.ToArray());
                            }
                        }
                    }
                    catch { }
                    _tmrPrune.FireInSecs(kPRUNE_SECONDS);
                    break;
                }
                //...................................................................
                case BEvent.EvType.Public:
                case BEvent.EvType.Entry:
                case BEvent.EvType.Exit:
                case BEvent.EvType.Init:
                case BEvent.EvType.None:
                {
                    break;
                }
            }
        }
    }
}
