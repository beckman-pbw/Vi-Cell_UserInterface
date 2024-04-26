using System;

namespace ApiProxies.Misc
{
    /// <summary>
    /// This interface is an inheritance root for types that store values as a result of something.
    /// </summary>
    public interface IResultStore
    {
    }

    /// <summary>
    /// Interface that defines properties to hold values that arise as a result of an operation being invoked.
    /// </summary>
    public interface IInvocationResult<TResult> : IResultStore
    {
        Tuple<TResult> Results { get; }
    }

    /// <summary>
    /// Interface that defines properties to hold values that arise as a result of an operation being invoked.
    /// </summary>
    public interface IInvocationResult<TResult1, TResult2> : IResultStore
    {
        Tuple<TResult1, TResult2> Results { get; }
    }

    /// <summary>
    /// Interface that defines properties to hold values that arise as a result of an operation being invoked.
    /// </summary>
    public interface IInvocationResult<TResult1, TResult2, TResult3> : IResultStore
    {
        Tuple<TResult1, TResult2, TResult3> Results { get; }
    }

    /// <summary>
    /// Interface that defines properties to hold values that arise as a result of an operation being invoked.
    /// </summary>
    public interface IInvocationResult<TResult1, TResult2, TResult3, TResult4> : IResultStore
    {
        Tuple<TResult1, TResult2, TResult3, TResult4> Results { get; }
    }

    /// <summary>
    /// Interface that defines properties to hold values that arise as a result of an operation being invoked.
    /// </summary>
    public interface IInvocationResult<TResult1, TResult2, TResult3, TResult4, TResult5> : IResultStore
    {
        Tuple<TResult1, TResult2, TResult3, TResult4, TResult5> Results { get; }
    }
}