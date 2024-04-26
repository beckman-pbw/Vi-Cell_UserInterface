using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace BAFW
{
    public class BHsm
    {
        /// <summary>
        /// This is the definition of a state, a function that returns a state.
        /// </summary>
        /// <param name="ev">The event for the state to process</param>
        /// <returns>The parent state or null if the event was handled.</returns>
        public delegate State State(BEvent ev);

        #region member variables
        private Control _winCtrl = null;
        private State _currentState = null;
        private State _activeState = null;

        // These are member variables to reduce the overhead of creating new variables each time we transition
        // These could be local variables in DoTransition() but that is frequently called 
        // It's less overhead to clear the variables before use than to create new variables
        private List<State> _targetAncestors = new List<State>();
        private List<State> _activeAncestors = new List<State>();
        private List<State> _entryList = new List<State>();
        private List<State> _exitList = new List<State>();
        #endregion

        #region constructors
        /*
         * ******************************************************************
         * \brief
         */
        protected BHsm()
        {
            _winCtrl = null;
            _currentState = null;
            _activeState = null;
        }

        /*
         * ******************************************************************
         * \brief
         */
        protected BHsm(Control winCtrl)
        {
            _winCtrl = winCtrl;
            _currentState = null;
            _activeState = null;
        }
        #endregion


        /*
         * ******************************************************************
         * \brief Used on the initial transition to a state. 
         */
        protected void SetState(State target) { _currentState = target; }


        /// <summary>
        /// For an Active Object, Set the root state after the constructor is done and before starting the thread. 
        /// For an Orthogonal Region, set the root state in the constructor tne call InitStateMachine() also in the constructor. 
        /// </summary>
        /// <param name="rootState">The root state of the HSM</param>
        protected void SetRootState(State rootState)
        {
            _currentState = rootState;
            _activeState = rootState;
        }

        #region initialize
        private delegate void MarshalInitHSM(); // used to mashal the init onto the UI thread
        /*
         * ******************************************************************
         * \brief Used to initialize an Active Object's state machine. This is 
         * called at the start of the Ao's thread. The on the UI thread if needed
         */
        internal bool ThreadStarted_InitHSM()
        {
            try
            {
                if (_winCtrl != null)
                {
                    if (_winCtrl.InvokeRequired)
                    {
                        MarshalInitHSM MarshalInitDelegate = new MarshalInitHSM(InitStateMachine);
                        _winCtrl.Invoke(MarshalInitDelegate, null);
                        return true;
                    }
                }
                InitStateMachine();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return false;
        }

        /*
         * ******************************************************************
         * \brief Initialize the state machine 
         */
        protected void InitStateMachine()
        {
            State tempState = _currentState;
            EnterState(tempState);
            SendInit(_activeState);
            tempState = _currentState;
            EnterState(tempState);
            while (SendInit(tempState) == null)
            {
                tempState = _currentState;
                EnterState(tempState);  // enter the substate
            }
        }
        #endregion

        #region helper_functions
        private static BEvent sEV_NONE = new BEvent(BEvent.EvType.None, 0);
        private static BEvent sEV_INIT = new BEvent(BEvent.EvType.Init, 0);
        private static BEvent sEV_ENTRY = new BEvent(BEvent.EvType.Entry, 0);
        private static BEvent sEV_EXIT = new BEvent(BEvent.EvType.Exit, 0);
        private State GetParent(State state) { return state(sEV_NONE); }
        private void EnterState(State state) { state(sEV_ENTRY); }
        private void ExitState(State state) { state(sEV_EXIT); }
        private State SendInit(State state) { return state(sEV_INIT); }
        #endregion

        #region deliver_events
        /*
         * ******************************************************************
         * \brief
         */
        public bool DeliverEvent(BEvent ev)
        {
            try
            {
                if (_winCtrl != null)
                {
                    if (_winCtrl.InvokeRequired)
                    {
                        object[] args = new object[] { ev };
                        MethodInvoker mmi;
                        mmi = (MethodInvoker)delegate { Deliver(ev); };
                        _winCtrl.Invoke(mmi, args);
                        return true;
                    }
                    else if (_winCtrl.IsDisposed)
                    {
                        return true;
                    }
                }
                Deliver(ev);
            }
            catch { }
            return false;
        }

        /*
         * ******************************************************************
         * \brief
         */
        private void Deliver(BEvent ev)
        {
            _activeState = _currentState;
            while (_activeState != null)
            {
                try
                {
                    _activeState = _activeState(ev);
                }
                catch (System.Threading.ThreadAbortException ex)
                {
                    try { Debug.WriteLine("BHsm::Deliver ThreadAbortException - " + ex.Message); } catch { }
                }
                catch (Exception ex)
                {
                    try
                    {
                        string strError = "BHsm::Deliver - " + this.ToString() + "::" + this._activeState.Method.ToString() + " - state threw exception\n";
                        MessageBox.Show(strError + "\nShutting down application\n\n" + ex.Message, "Exception ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch { }
                    try { Debug.WriteLine(ex.Message); } catch { }
                    Application.Exit();
                }
            }
            return;
        }
        #endregion

        #region transition
        /*
         * ******************************************************************
         * \brief Transition from one state to another. 
         * Transition from the "active" state to the target state. The active
         * state is the state handling the current event. 
         * 
         */
        protected void DoTransition(State targetState)
        {
            State targetSuperState = GetParent(targetState);
            State activeSuperState = GetParent(_activeState);
            try
            {
                // Exit all states from the current state up to active state
                var superState = _currentState;
                while (superState != _activeState)
                {
                    ExitState(superState);
                    superState = GetParent(superState);
                }

                //
                // At this point we are in the active state
                //

                // Clear our member variables before use
                _entryList.Clear();
                _exitList.Clear();

                if (_activeState == targetState) // Transition to self
                {
                    _exitList.Add(_activeState);
                    _entryList.Add(targetState);
                }
                else if (_activeState == targetSuperState) // target is active state's child
                {
                    // We only need to transition down to the target state
                    _entryList.Add(targetState);
                }
                else if (activeSuperState == targetSuperState) // active and target are siblings - same parent
                {
                    _exitList.Add(_activeState);
                    _entryList.Add(targetState);
                }
                else if (activeSuperState == targetState) // Target state is active state's parent
                {
                    _exitList.Add(_activeState);
                    _exitList.Add(targetState);
                    _entryList.Add(targetState);
                }
                else
                {
                    // A more complex relationship
                    // get a list of the active state's ancestors 
                    // and a list of the target state's ancestors
                    // find the first common ancestor in the two lists

                    // Clear our member variables before use
                    _targetAncestors.Clear();
                    _activeAncestors.Clear();

                    // All active state's ancestors 
                    _activeAncestors.Add(_activeState);
                    superState = GetParent(_activeState);
                    while (superState != null)
                    {
                        _activeAncestors.Add(superState);
                        superState = GetParent(superState);
                    }

                    // All target state's ancestors 
                    _targetAncestors.Add(targetState);
                    superState = GetParent(targetState);
                    while (superState != null)
                    {
                        _targetAncestors.Add(superState);
                        superState = GetParent(superState);
                    }

                    // Find the first common ancestor 
                    bool found = false;
                    foreach (var targ in _targetAncestors)
                    {
                        _entryList.Add(targ);
                        _exitList.Clear();
                        foreach (var src in _activeAncestors)
                        {
                            if (src == targ)
                            {
                                found = true;
                                break;
                            }
                            _exitList.Add(src);
                        }
                        if (found)
                            break;
                    }
                    if (!found)
                    {
                        throw new Exception("fatal error - bad state machine implementation"); // fatal error - bad state machine implementation 
                    }
                }

                // We collected the entry list in reverse order
                if (_entryList.Count > 1)
                {
                    _entryList.Reverse();
                    _entryList.RemoveAt(0);
                }
                // exit the states in the exit list
                foreach (var st in _exitList)
                {
                    ExitState(st);

                }
                // enter into the states in the entry list 
                foreach (var st in _entryList)
                {
                    EnterState(st);

                }
                _currentState = targetState;
                // Now follow the INITs - maybe more than one
                while (SendInit(targetState) == null)
                {
                    targetState = _currentState;
                    EnterState(targetState); // enter target state
                }
                return;
            }

            catch (Exception ex)
            {
                string strError = "BHsm::DoTransition - " + this.ToString() + " st = " + this._activeState.ToString() + " - state threw exception\n";
                MessageBox.Show(strError + ex.Message, "Exception ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine(ex.Message);
                Application.Exit();
            }
        }

        #endregion

    }
}
