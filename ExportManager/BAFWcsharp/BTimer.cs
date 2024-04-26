using System;

namespace BAFW
{
    /*
     * ******************************************************************
     * \brief Simple low resolution timer. 
     */
    public class BTimer
    {

        #region member_variables
        private BTimerEvent _timerEvent;
        private BEventQueue _evQueue;
        private DateTime _expiresAt;
        private DateTime _armTime;
        private bool _armed;
        private bool _inserted;
        #endregion

        /*
         * ******************************************************************
         * \brief Create a new timer         
         * \param evQueue - event queue to post timer events to
         * \param timerId - the event Id that is used when a timer event is generated                 
         */
        public BTimer(BEventQueue evQueue, UInt32 timerId)
        {
            _timerEvent = new BTimerEvent(timerId);
            _evQueue = evQueue;
            _armed = false;
            _inserted = false;
        }

        /*
         * ******************************************************************
         * \brief The destructor cleans up what's left. 
         */
        ~BTimer()
        {
            Disarm();
            if (_inserted)
            {
                BAppFW.RemoveTimer(this);
                _inserted = false;
            }
        }

        /*
         * ******************************************************************
         * \brief The Id that will be used for generated events.
         */
        public UInt32 Id { get { return _timerEvent.Id; } }

        /*
         * ******************************************************************
         * \brief Disarm the timer if armed and remove timer event if in event Q. 
         */
        public void Disarm()
        {
            _armed = false;
            _evQueue.Remove(_timerEvent);
        }

        /*
         * ******************************************************************
         * \brief Post a timer event in the given number of milli-seconds. 
         * \param ms - number of milli-seconds until the timer expires
         * \param userData - data available in the timer event
         */
        public void FireIn(UInt32 ms, UInt32 userData = 0)
        {
            if (_armed == false)
            {
                _armTime = DateTime.Now;
                _expiresAt = _armTime.AddMilliseconds(ms);
                if (_inserted == false)
                {
                    _inserted = BAppFW.AddTimer(this);
                }
                if (_inserted)
                {
                    _armed = true;
                    _timerEvent.SetAppData(userData);
                }
            }
        }

        /*
         * ******************************************************************
         * \brief Post a timer event in the given number of seconds. 
         * \param seconds - number of seconds until the timer expires
         * \param userData - data available in the timer event
         */
        public void FireInSecs(UInt32 seconds, UInt32 userData = 0)
        {
            if (_armed == false)
            {
                _armTime = DateTime.Now;
                _expiresAt = _armTime.AddMilliseconds(seconds * 1000);
                if (_inserted == false)
                {
                    _inserted = BAppFW.AddTimer(this);
                }
                if (_inserted)
                {
                    _armed = true;
                    _timerEvent.SetAppData(userData);
                }
            }
        }

        /*
         * ******************************************************************
         * \brief Number of milli-seconds until the timer will expire
         * \param timeNow - current time
         */
        public Double TimeTillExpire(DateTime timeNow)
        {
            if (_armed)
            {
                double msRemain = _expiresAt.Subtract(timeNow).TotalMilliseconds;
                if (msRemain <= 0)
                {
                    return 0.0;
                }
                return msRemain;
            }
            else
            {
                return -1.1;
            }
        }

        /*
         * ******************************************************************
         * \brief Post an event to the event Q is the timer has expired.
         * \param timeNow - current time
         */
        internal void PostEventIfExpired(DateTime timeNow)
        {
            if (_armed)
            {
                if (_expiresAt.Subtract(timeNow).TotalMilliseconds <= 0)
                {
                    if (_evQueue.Put(_timerEvent, BEventQueue.WaitType.NoWait) == BEventQueue.Status.Ok)
                    {
                        _armed = false;
                    }
                }
            }
        }

    }
}
