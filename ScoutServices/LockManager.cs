using ScoutServices.Enums;
using ScoutServices.Interfaces;
using System;
using System.Diagnostics;
using System.Reactive.Subjects;

namespace ScoutServices
{
    public class LockManager : ILockManager
    {
        #region Fields

        private static readonly object _statusLock = new object();
        private static readonly object _lockOwnerLock = new object();
        private static readonly object _managerLock = new object();

        private readonly Subject<LockResult> _systemLockStateChangeSubject;

        private bool _isLocked;
        private string _userIdThatOwnsLock;

        #endregion

        public LockManager()
        {
            _systemLockStateChangeSubject = new Subject<LockResult>();
            _userIdThatOwnsLock = string.Empty;
        }

        #region Methods

        public bool IsLocked()
        {
            lock (_statusLock)
            {
                return _isLocked;
            }
        }

        public string UserThatOwnsLock()
        {
            lock (_lockOwnerLock)
            {
                return _userIdThatOwnsLock;
            }
        }

        public virtual bool OwnsLock(string userId) // virtual for unit tests and mocking
        {
            if (string.IsNullOrEmpty(userId))
                return false;
            if (!IsLocked())
                return false;
            if (UserThatOwnsLock().Equals(userId))
                return true;

            return false;
        }

        public IObservable<LockResult> SubscribeStateChanges()
        {
            lock (_managerLock)
            {
                return _systemLockStateChangeSubject;
            }
        }

        public void PublishAutomationLock(LockResult state, string userThatPerformedAction)
        {
            try
            {
                lock (_statusLock)
                {
                    if (state == LockResult.Locked)
                        _isLocked = true;
                    else if (state == LockResult.Unlocked)
                        _isLocked = false;
                }

                lock (_lockOwnerLock)
                {
                    if (state == LockResult.Locked)
                        _userIdThatOwnsLock = userThatPerformedAction;
                    else
                        _userIdThatOwnsLock = string.Empty;
                }

                lock (_managerLock)
                {
                    _systemLockStateChangeSubject.OnNext(state);
                }
            }
            catch (Exception e)
            {
                lock (_managerLock)
                {
                    _systemLockStateChangeSubject.OnError(e);
                }
            }
        }

        #endregion
    }
}
