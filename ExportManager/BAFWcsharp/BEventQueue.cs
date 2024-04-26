using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

namespace BAFW
{
    /*
     * ******************************************************************
     * \brief
     */
    public class BEventQueue
    {
        public enum Status
        {
            Ok = 0,
            ERR_WouldBlock,
            ERR_QFull,
            ERR_BadParam
        };
        public enum WaitType
        {
            NoWait = 0,
            WaitForever
        };

        #region member_variables
        List<BEvent> _theQ;
        internal Semaphore _sema;
        private bool _showError = true;
        private UInt32 _maxCount;
        #endregion

        /*
         * ******************************************************************
         * \brief
         */
        public BEventQueue(UInt32 maxCount)
        {
            _maxCount = maxCount;
            _theQ = new List<BEvent>();
            _sema = new Semaphore(0, (int)_maxCount);
        }

        /*
         * ******************************************************************
         * \brief
         */
        public void Close()
        {
            try
            {
                if (_sema != null)
                {
                    _sema.Release(5);
                    _sema.Dispose();
                    _sema = null;
                }
            }
            catch { }
            try
            {
                if (_theQ != null)
                {
                    _theQ.Clear();
                    _theQ = null;
                }
            }
            catch { }
        }

        /*
         * ******************************************************************
         * \brief
         */
        public Status Put(BEvent ev, WaitType wait)
        {
            Status status = Status.ERR_QFull;

            try
            {
                if (_theQ == null)
                {
                    Debug.WriteLine("QEventQueue::Put - attempt to put into closed queue");
                    return Status.ERR_BadParam;
                }
                if (_sema == null)
                {
                    Debug.WriteLine("QEventQueue::Put - attempt to put into closed queue");
                    return Status.ERR_BadParam;
                }
            }
            catch { }

            try
            {
                if ((ev == null))
                {
                    return Status.ERR_BadParam;
                }

                if (wait == WaitType.NoWait)
                {
                    if (!BAppFW.TryEnterCs())
                    {
                        return Status.ERR_WouldBlock;
                    }
                }
                else
                {
                    BAppFW.EnterCs();
                }
                if (_theQ != null)
                {
                    if (_theQ.Count < _maxCount)
                    {
                        _theQ.Add(ev);
                        status = Status.Ok;
                        // Incrementing the semaphore unblocks anyone who is waiting for an event from the queue.
                        try
                        {
                            _sema.Release();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.Message);
                            status = Status.ERR_QFull;
                        }
                    }
                    else
                    {
                        status = Status.ERR_QFull;
                    }
                }
                BAppFW.LeaveCs();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                if (_showError)
                {
                    string message = "QEventQueue::Put - threw exception\nShut down application?\n" + ex.Message;
                    message += "\n Event ID = " + ev.Id;
                    DialogResult dr = MessageBox.Show(message, "Exception ERROR", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                    else
                    {
                        _showError = false;
                    }
                }
                BAppFW.LeaveCs();
            }
            return status;
        }

        /*
         * ******************************************************************
         * \brief
         */
        public BEvent Get(WaitType wait)
        {

            try
            {
                if (_theQ == null)
                {
                    Debug.WriteLine("QEventQueue::Get - attempt to get from closed queue");
                    return null;
                }
                if (_sema == null)
                {
                    Debug.WriteLine("QEventQueue::Get - attempt to get from closed queue");
                    return null;
                }
            }
            catch { }


            try
            {
                if (wait == WaitType.NoWait)
                {
                    if (_sema.WaitOne(0) == false)
                    {
                        return null;
                    }
                }
                else
                {
                    _sema.WaitOne();
                }
            }
            catch
            {
                Debug.WriteLine("QEventQueue::Get - wait exception");
                return null;
            }

            BAppFW.EnterCs();
            BEvent ev = null;
            try
            {
                if (_theQ == null) { return null; }
                if (_theQ.Count > 0)
                {
                    ev = _theQ[0];
                    _theQ.RemoveAt(0);
                }
            }
            catch { }
            BAppFW.LeaveCs();
            return ev;
        }

        /*
         * ******************************************************************
         * \brief
         */
        internal void Remove(BEvent ev)
        {
            BAppFW.EnterCs();
            try
            {
                if (_theQ != null)
                {
                    _theQ.Remove(ev);
                }
            }
            catch { }
            BAppFW.LeaveCs();
        }


    }
}
