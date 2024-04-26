using log4net;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ScoutUtilities.Common
{
    public abstract class Disposable : IDisposable
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #region Desctructors

        ~Disposable()
        {
            Dispose(false);
        }

        #endregion

        #region Properties & Fields

        private Subject<Unit> _whenDisposedSubject;

        public IObservable<Unit> WhenDisposed
        {
            get
            {
                if (IsDisposed)
                {
                    return Observable.Return(Unit.Default);
                }

                if (_whenDisposedSubject == null)
                {
                    _whenDisposedSubject = new Subject<Unit>();
                }

                return _whenDisposedSubject.AsObservable();
            }
        }


        public bool IsDisposed { get; private set; }

        #endregion

        #region Methods

        public virtual void Dispose()
        {
            // Dispose all managed and unmanaged resources.
            Dispose(true);
            // Take this object off the finalization queue and prevent finalization code for this
            // object from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void DisposeManaged()
        {
        }
        
        protected virtual void DisposeUnmanaged()
        {
        }

        protected void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!IsDisposed)
            {
                // If disposing managed and unmanaged resources.
                if (disposing)
                {
                    DisposeManaged();
                }

                DisposeUnmanaged();
                IsDisposed = true;
                if (_whenDisposedSubject != null)
                {
                    // Raise the WhenDisposed event.
                    _whenDisposedSubject.OnNext(Unit.Default);
                    _whenDisposedSubject.OnCompleted();
                    _whenDisposedSubject.Dispose();
                }
            }
        }

        #endregion
    }
}