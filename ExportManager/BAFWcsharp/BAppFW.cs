using System;
using System.Collections.Generic;
using System.Linq;


namespace BAFW
{
    public class BAppFW
    {
        // ******************************************************************
        static public void Delay(int delayTimeMS)
        {
            DateTime startTime = DateTime.Now;
            do
            {
                System.Threading.Thread.Sleep(5);
                System.Windows.Forms.Application.DoEvents();
            } while (DateTime.Now.Subtract(startTime).TotalMilliseconds < delayTimeMS);
        }

        #region member_variables
        static System.Threading.Timer _tickTimer = null;
        static List<BTimer> _timerList;
        static List<BActive> _allEventSubList = new List<BActive>();
        static List<BEventSubScriberList> _subscriberList = new List<BEventSubScriberList>();
        static private System.Threading.Mutex _csMutex = new System.Threading.Mutex();
        static bool _active = true;
        #endregion

        public static bool Active { get { return _active; } }

        /*
         * ******************************************************************
         * \brief
         */
        internal static void EnterCs() { try { _csMutex.WaitOne(); } catch { } }
        /*
         * ******************************************************************
         * \brief
         */
        internal static bool TryEnterCs() { return _csMutex.WaitOne(0); }
        /*
         * ******************************************************************
         * \brief
         */
        internal static void LeaveCs() { _csMutex.ReleaseMutex(); }

        /*
         * ******************************************************************
         * \brief
         */
        public static void Shutdown()
        {
            _active = false;
            Delay(15);
        }

        /*
         * ******************************************************************
         * \brief
         */
        static public void Publish(BPublicEvent ev)
        {
            if (ev == null) return;            
            // By entering the critical section here, we can ensure that all threads will block 
            // until all that have subscribed to this event have been posted to.
            EnterCs();
            var list = FindSubscriberList(ev);
            if (list != null)
            {
                list.Publish(ev);
            }
            // Now post the event to objects that subscribe to all events
            foreach (var ao in _allEventSubList)
            {
                ao.PostEvent(ev);
            }
            LeaveCs();
        }

        /*
         * ******************************************************************
         * \brief
         */
        static public void Subscribe(BActive active, BPublicEvent pubEv)
        {
            EnterCs();
            var list = FindSubscriberList(pubEv);
            if (list == null)
            {
                list = new BEventSubScriberList(pubEv);
                list.Add(active);
                _subscriberList.Add(list);
            }
            else
            {
                if (list.Contains(active) == false)
                {
                    list.Add(active);
                }
            }
            LeaveCs();
            return;
        }


        /*
         * ******************************************************************
         * \brief
         */
        static public void UnsubscribeAll(BActive active)
        {
            EnterCs();
            foreach (var esl in _subscriberList)
            {
                esl.Remove(active);
            }
            _allEventSubList.Remove(active);
            LeaveCs();
        }

        /*
         * ******************************************************************
         * \brief Subscribes to all public events
         */
        static public void SubscribeAll(BActive active)
        {
            EnterCs();
            if (_allEventSubList.Contains(active) == false)
            {
                _allEventSubList.Add(active);
            }
            LeaveCs();
        }


        /*
         * ******************************************************************
         * \brief
         */
        static public void Unsubscribe(BActive active, BPublicEvent pubEv)
        {
            EnterCs();
            var list = FindSubscriberList(pubEv);
            if (list != null)
            {
                list.Remove(active);
            }
            LeaveCs();
            return;
        }


        /*
         * ******************************************************************
         * \brief Timer resolution is roughly two times this value. 
         */
        public const int kTIMER_PERIOD_MS = 20;

        /*
         * ******************************************************************
         * \brief
         */
        static internal bool AddTimer(BTimer tmr)
        {
            if (_timerList == null)
            {
                _timerList = new List<BTimer>();
            }
            if (_timerList.Contains(tmr)) { return true; }
            EnterCs();
            _timerList.Add(tmr);
            LeaveCs();
            if (_tickTimer == null)
            {
                _tickTimer = new System.Threading.Timer(TimerTickThread, null, kTIMER_PERIOD_MS, kTIMER_PERIOD_MS);
            }
            return true;
        }

        /*
         * ******************************************************************
         * \brief
         */
        static internal void RemoveTimer(BTimer tmr)
        {
            EnterCs();
            if (_timerList.Contains(tmr))
            {
                _timerList.Remove(tmr);
            }
            LeaveCs();
            return;
        }


        /*
         * ******************************************************************
         * \brief
         */
        static private void TimerTickThread(Object stateInfo)
        {
            if (!Active) { return; }
            EnterCs();
            DateTime timeNow = DateTime.Now;
            foreach (var tmr in _timerList)
            {
                tmr.PostEventIfExpired(timeNow);
            }
            LeaveCs();
        }

        /*
         * ******************************************************************
         * \brief
         */
        static BEventSubScriberList FindSubscriberList(BPublicEvent ev)
        {
            Type findType = ev.GetType();
            foreach (var esl in _subscriberList)
            {
                if (!esl.evType.Equals(findType))
                    continue;
                if (esl.Id == ev.Id)
                {
                    return esl;
                }
            }
            return null;
        }
    }
}
