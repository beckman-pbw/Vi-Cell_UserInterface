using ScoutServices.Enums;
using System;

namespace ScoutServices.Interfaces
{
    public interface ILockManager
    {
        bool IsLocked();

        IObservable<LockResult> SubscribeStateChanges();

        void PublishAutomationLock(LockResult state, string userThatPerformedAction);

        /// <summary>
        /// Returns the userId for the user that currently owns the lock.
        /// </summary>
        /// <returns></returns>
        string UserThatOwnsLock();

        /// <summary>
        /// Returns 'true' if 'userId' is the user that currently owns the lock.
        /// Returns 'false' if the system is not locked.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        bool OwnsLock(string userId);
    }
}
