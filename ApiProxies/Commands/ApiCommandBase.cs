using ApiProxies.Generic;
using ApiProxies.Misc;
using JetBrains.Annotations;
using log4net;
using ScoutUtilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace ApiProxies.Commands
{
    /// <summary>
    /// Base class that acts as a proxy to an API call.  Every instance of a ApiCommandBase is meant to be
    /// invoked only one time via the <see cref="IApiCommand"/>.  
    /// </summary>
    public abstract class ApiCommandBase : IApiCommand
    {
        protected static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly object _lockObj = new object();
        protected volatile bool _isInvoked; // To detect redundant calls
        protected bool _isDisposed; // To detect redundant calls
        protected bool _isFinalized; // To detect redundant calls

        private readonly List<IntPtr> _unmanagedItems = new List<IntPtr>();

        /// <summary>
        /// Gets/sets a value indicating whether this API method type is responsible for allocating and freeing
        /// memory.
        /// </summary>
        public bool ManagesMemory { get; protected set; }

        /// <summary>
        /// Gets/sets a value indicating whether this class automatically releases any unmanaged memory that it holds.
        /// Defaults to true.
        /// </summary>
        public bool AutoReleaseMemory { get; set; } = true;

        /// <summary>
        /// Gets a value indicating whether this proxy method has been invoked yet. 
        /// Not thread-safe (volatile).
        /// </summary>
        public bool IsInvoked
        {
            get { return _isInvoked; }
        }

        /// <summary>
        /// Gets/sets the result of this proxy being invoked.
        /// </summary>
        public HawkeyeError Result { get; protected set; }

        /// <summary>
        /// Gets a list of exceptions caused by a call to Invoke. The list may be empty.
        /// </summary>
        public List<Exception> InvocationExceptions { get; } = new List<Exception>();

        /// <summary>
        /// Gets a value that indicates whether this IApiCommand experienced an exception during invocation.
        /// </summary>
        public bool IsFaulted => InvocationExceptions.Any();

        /// <summary>
        /// Finalizer is provided because descendents of this class may need to clean-up
        /// unmanaged memory.
        /// </summary>
        ~ApiCommandBase()
        {
            Dispose(false);
        }

        /// <summary>
        /// This code added to correctly implement the disposable pattern.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // Because the finalizer is overridden.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Allocates unmanaged memory for the given argument using Marshal.AllocCoTaskMem(),
        /// then adds the resulting pointer to the <see cref="_unmanagedItems"/> collection
        /// for automatic clean-up.
        /// </summary>
        /// <param name="memSize">the desired size of the unmanaged memory block</param>
        /// <returns>IntPtr pointing to the new block of unmanaged memory</returns>
        protected IntPtr AllocateAndTrack(int memSize)
        {
            ManagesMemory = true;
            var intPtr = Marshal.AllocCoTaskMem(memSize);
            _unmanagedItems.Add(intPtr);
            return intPtr;
        }

        /// <summary>
        /// Allocates unmanaged memory for the given collection argument using Marshal.AllocHGlobal(),
        /// then adds the resulting pointer to the <see cref="_unmanagedItems"/> collection
        /// for automatic clean-up.
        /// </summary>
        /// <typeparam name="TStruct"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        protected IntPtr AllocateAndTrack<TStruct>(List<TStruct> items) where TStruct : struct
        {
            ManagesMemory = true;

            var structSize = Marshal.SizeOf(typeof(TStruct));
            IntPtr arrayPtr = Marshal.AllocHGlobal(structSize * items.Count);
            IntPtr iterator = arrayPtr;
            _unmanagedItems.Add(arrayPtr);

            for (int i = 0; i < items.Count; i++)
            {
                Marshal.StructureToPtr(items[i], iterator, false);
                iterator += structSize;
            }

            return arrayPtr;
        }

        /// <summary>
        /// Allocates unmanaged memory for the given argument using Marshal.StringToHGlobalAnsi(),
        /// then adds the resulting pointer to the <see cref="_unmanagedItems"/> collection
        /// for automatic clean-up.
        /// </summary>
        /// <param name="toHGlobalAnsi">the string to marshal into unmanaged memory</param>
        /// <returns>IntPtr pointing to the new block of unmanaged memory</returns>
        protected IntPtr AllocateAndTrack(string toHGlobalAnsi)
        {
            ManagesMemory = true;
            var intPtr = Marshal.StringToHGlobalAnsi(toHGlobalAnsi);
            _unmanagedItems.Add(intPtr);
            return intPtr;
        }

        /// <summary>
        /// This is intended to free any memory that was not tracked in the <see cref="_unmanagedItems"/> collection,
        /// including memory that is allocated in native linked libraries.
        /// </summary>
        protected virtual void FreeAdditionalUnmanagedMemory()
        {
            
        }

        /// <summary>
        /// This may not require any overrides.
        /// </summary>
        protected virtual void DisposeInternal()
        {
            // empty base impl
        }

        /// <summary>
        /// Thread-safe.  This frees memory that was allocated locally in the application domain, as well as memory
        /// that was allocated in any native linked libraries.
        /// </summary>
        protected void FreeUnmanagedMemory()
        {
            lock (_lockObj)
            {
                if (_isFinalized || !_isInvoked)
                {
                    return;
                }

                // Free locally allocated memory
                foreach (var unmanagedItem in _unmanagedItems)
                {
                    Marshal.FreeCoTaskMem(unmanagedItem);
                }

                _unmanagedItems.Clear();

                try
                {
                    // Free memory allocated in C++ libs.
                    FreeAdditionalUnmanagedMemory();
                }
                catch (Exception ex)
                {
                    InvocationExceptions.Add(ex);
                    ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_FREE_UNMANAGED_MEMORY");
                }

                _isFinalized = true;
            }
        }

        /// <summary>
        /// Thread-safe dispose that handles managed and unmanaged objects.
        /// </summary>
        /// <param name="disposing"></param>
        protected void Dispose(bool disposing)
        {
            lock (_lockObj)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        DisposeInternal();
                    }

                    _isDisposed = true;
                }

                if (ManagesMemory)
                {
                    FreeUnmanagedMemory();
                }
            }
        }

        /// <summary>
        /// Invokes this class' API method if it has not been invoked yet.  Thread-safe.
        /// If <see cref="AutoReleaseMemory"/> is set, this will also free any unmanaged memory.
        /// </summary>
        [MustUseReturnValue("Use HawkeyeError")] public HawkeyeError Invoke()
        {
            lock (_lockObj)
            {
                if (_isInvoked)
                {
                    throw new InvalidOperationException(nameof(ApiCommandBase) + " has already been invoked.");
                }

                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(ApiCommandBase));
                }

                try
                {
                    InvokeInternal();
                }
                catch (Exception ex)
                {
                    InvocationExceptions.Add(ex);
                    ExceptionHelper.LogException(ex, "LID_EXCEPTIONMSG_ERROR_ON_INVOKE");
                    Result = HawkeyeError.eNoneFound;
                }

                _isInvoked = true;

                if (AutoReleaseMemory && ManagesMemory)
                {
                    FreeUnmanagedMemory();
                }
            }

            return Result;
        }

        protected abstract void InvokeInternal();
    }
}