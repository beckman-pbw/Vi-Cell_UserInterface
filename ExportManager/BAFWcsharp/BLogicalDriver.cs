using System;
using System.Diagnostics;
using System.Threading;

namespace BAFW
{
    abstract public class BLogicalDriver
    {

        /*
         * ******************************************************************
         * \brief Post the given public event to the event queue
         */
        public bool PostEvent(BPublicEvent ev)
        {
            if (_terminateThread)
            {
                return false;
            }
            return (_evQueue.Put((BEvent)ev, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }


        /*
         * ******************************************************************
         * \brief Process the next event from the event queue. 
         * Each LD must implement this function.
         */
        abstract protected void ProcessEvent(BEvent ev);



        /*
         * ******************************************************************
         * \brief Post a private event to the event queue
         */
        protected bool PostEvent(BPrivateEvent ev)
        {
            if (_terminateThread)
            {
                return false;
            }
            return (_evQueue.Put((BEvent)ev, BEventQueue.WaitType.WaitForever) == BEventQueue.Status.Ok);
        }

        /*
         * ******************************************************************
         * \brief Get the event queue for this LD
         */
        public BEventQueue GetEventQueue()
        {
            return _evQueue;
        }

        /*
         * ******************************************************************
         * \brief Starts the thread for this LD. 
         * The derived object's constructor must call this function, this will
         * ensure all setup is completed before the thread is started.
         */
        protected bool StartThread(String name)
        {
            _taskThread.Start();
            return true;
        }

        /*
         * ******************************************************************
         * \brief
         */
        ~BLogicalDriver()
        {
            try
            {
                StopThreadHard();
            }
            catch { }
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected BLogicalDriver(UInt32 queueSize = 1024)
            : base()
        {
            _evQueue = new BEventQueue(queueSize);
            _taskThread = new Thread(ObjectThreadTask);
        }

        public virtual void Shutdown()
        {
            if (!_terminateThread)
            {
                _terminateThread = true;
                BPublicEvent ev = new BPublicEvent(0, 0);
                _evQueue.Put(ev, BEventQueue.WaitType.WaitForever);
            }
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected virtual void StopThreadHard()
        {
            if (!_terminateThread)
            {
                _terminateThread = true;

                try
                {
                    if (_taskThread != null)
                    {
                        _taskThread.Join(25);
                        _taskThread.Interrupt();
                        _taskThread.Abort();
                        _taskThread = null;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

                try
                {
                    if (_evQueue != null)
                    {
                        _evQueue.Close();
                        _evQueue = null;
                    }
                }
                catch { }

            }
        }

        #region member_variables
        private BEventQueue _evQueue;
        protected bool _terminateThread;
        private Thread _taskThread;
        #endregion

        /*
         * ******************************************************************
         * \brief The thread for this logical driver.
         * 
         */
        private void ObjectThreadTask()
        {

            // A short delay at thread start time
            System.Threading.Thread.Sleep(250);

            try
            {
                // Keep running until it is time to shutdown
                do
                {
                    // Block on the main event queue.
                    BEvent pEvent = null;
                    try
                    {
                        pEvent = _evQueue.Get(BEventQueue.WaitType.WaitForever);
                    }
                    catch { }
                    // Process all non-NULL events 
                    if ((!this._terminateThread) && (pEvent != null))
                    {
                        try
                        {
                            ProcessEvent(pEvent);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("BLogicalDriver::ObjectThreadTask-ProcessEvent-except" + ex.Message);
                        }
                        pEvent = null;
                    }
                    else
                    {
                        Debug.WriteLine("Event null: " + this.ToString());
                    }
                } while ((!this._terminateThread) && (BAppFW.Active));

            }
            catch (ThreadAbortException taex)
            {
                Debug.WriteLine("BLogicalDriver::ObjectThreadTask" + taex.Message);
            }
            catch (ThreadInterruptedException tiex)
            {
                Debug.WriteLine("BLogicalDriver::ObjectThreadTask" + tiex.Message);
            }
            return;
        }

    }
}
