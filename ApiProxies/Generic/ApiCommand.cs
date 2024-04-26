using System;
using ApiProxies.Commands;

namespace ApiProxies.Generic
{
    public abstract class ApiCommand<TParam1> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3, TParam4> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3, TParam4> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3, TParam4, TParam5> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3, TParam4, TParam5> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7> Arguments { get; protected set; }
    }

    public abstract class ApiCommand<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> : ApiCommandBase
    {
        /// <summary>
        /// Holds the arguments that have been supplied to the method during invocation.
        /// </summary>
        public Tuple<TParam1, TParam2, TParam3, TParam4, TParam5, TParam6, TParam7, TParam8> Arguments { get; protected set; }
    }

}