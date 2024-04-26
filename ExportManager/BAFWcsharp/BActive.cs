using System;
using System.Diagnostics;
using System.Threading;

namespace BAFW
{
    /*
     * ******************************************************************
     * \brief Active objects are an HSM with a thread of execution. 
     *        Each active object blocks waiting for external exvents. 
     */
    public class BActive : BHsm
    {
        #region member_variables
        private BEventQueue _externalQ;
        private BEventQueue _internalQ;
        private bool _stopThread = false;
        private Thread _taskThread;
        #endregion

        /*
         * ******************************************************************
         * \brief
         */
        protected BActive(UInt32 maxQCount = 1024, UInt32 maxIQCount = 32, System.Windows.Forms.Control winCtrl = null)
            : base(winCtrl)
        {
            _externalQ = new BEventQueue(maxQCount);
            _internalQ = new BEventQueue(maxIQCount);
            _taskThread = new Thread(AoThread);
        }

        /*
         * ******************************************************************
         * \brief
         */
        ~BActive()
        {
            StopThread();
        }


        /*
         * ******************************************************************
         * \brief
         */
        public BEventQueue GetEventQueue()
        {
            return _externalQ;
        }

        /*
         * ******************************************************************
         * \brief
         */
        public bool PostEvent(BPublicEvent ev)
        {
            if (_stopThread)
            {
                return false;
            }
            return (_externalQ.Put((BEvent)ev, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief
         */
        public bool PostEvent(BPrivateEvent ev)
        {
            if (_stopThread)
            {
                return false;
            }
            return (_externalQ.Put(ev, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief
         */
        public bool PostInternalEvent(BPrivateEvent ev)
        {
            if (_stopThread)
            {
                return false;
            }
            return (_internalQ.Put(ev, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief
         */
        public bool PostInternalEvent(UInt32 evId, UInt32 appData = 0)
        {
            if (_stopThread)
            {
                return false;
            }
            return (_internalQ.Put(new BPrivateEvent(evId, appData), BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected bool StartThread()
        {
            try
            {
                _taskThread.Start();
                return true;
            }
            catch { }
            return false;
        }


        /*
         * ******************************************************************
         * \brief
         */
        public virtual void Shutdown()
        {
            try
            {
                BAppFW.UnsubscribeAll(this);
            }
            catch { }
            if (!_stopThread)
            {
                _stopThread = true;
                _externalQ.Put(new BPublicEvent(0), BEventQueue.WaitType.WaitForever);
            }
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected virtual void StopThread()
        {
            if (_stopThread)
            {
                // Already set
                return;
            }
            _stopThread = true;

            try
            {
                BAppFW.UnsubscribeAll(this);
            }
            catch { }

            try
            {
                if (_taskThread != null)
                {
                    _taskThread.Join(250);
                    _taskThread.Interrupt();
                    _taskThread.Abort();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            _taskThread = null;

            try
            {
                if (_externalQ != null)
                {
                    _externalQ.Close();
                    _externalQ = null;
                }
            }
            catch { }
        }


        /*
         * ******************************************************************
         * \brief - The "message" pump - event processing 
         * 
         */
        private void AoThread()
        {
            System.Threading.Thread.Sleep(250);
            int internalEventCount;
            try
            {
                try
                {
                    this.ThreadStarted_InitHSM();

                    while ((!this._stopThread) && (BAppFW.Active))
                    {
                        // Block on the main event queue.
                        BEvent pEvent = null;
                        try
                        {
                            pEvent = _externalQ.Get(BEventQueue.WaitType.WaitForever);
                        }
                        catch { }

                        if (this._stopThread || (BAppFW.Active == false))
                        {
                            if (pEvent != null)
                            {
                                pEvent = null;
                            }
                            return;
                        }

                        // No internal events are allowed between external events
                        // Let's do a quick check here. 
                        BEvent pIe = null;
                        try
                        {
                            pIe = _internalQ.Get(BEventQueue.WaitType.NoWait);
                        }
                        catch { }

                        if (pIe != null)
                        {
                            // Error - Internal event in queue before event processing.
                            pIe = null;
                        }

                        if (pEvent != null)
                        {
                            // Deliver the external event to the state machine.
                            DeliverEvent(pEvent);
                            pEvent = null;

                            try
                            {
                                pIe = _internalQ.Get(BEventQueue.WaitType.NoWait);
                            }
                            catch { }
                            internalEventCount = 0;
                            while (pIe != null)
                            {
                                if (_stopThread || (BAppFW.Active == false))
                                {
                                    pIe = null;
                                    return;
                                }
                                DeliverEvent(pIe);
                                pIe = null;
                                pIe = _internalQ.Get(BEventQueue.WaitType.NoWait);
                                if (internalEventCount++ > 32)
                                {
                                    Debug.WriteLine("Possible infinite internal event loop, objID" + this.ToString());
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Public event null" + this.ToString());
                        }
                    }
                    return;
                }
                catch (ThreadAbortException taex)
                {
                    Debug.WriteLine("BActive::ObjectThreadTask ThreadAbortException - " + taex.Message);
                }
                catch (ThreadInterruptedException tiex)
                {
                    Debug.WriteLine("BActive::ObjectThreadTask ThreadInterruptedException - " + tiex.Message);
                }
            }
            catch
            {
            }
        }


    }

}
