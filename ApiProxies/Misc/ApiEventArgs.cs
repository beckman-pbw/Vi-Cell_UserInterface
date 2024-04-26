using System;
using System.Collections.Generic;

namespace ApiProxies.Misc
{
    public class ApiEventArgs : EventArgs
    {
        public ApiEventType EventType { get; set; }

        public List<Exception> Exceptions { get; set; }
    }

    public class ApiEventArgs<TArg1> : ApiEventArgs
    {
        public TArg1 Arg1 { get; set; }

        public ApiEventArgs()
        {
        }

        public ApiEventArgs(TArg1 arg1)
        {
            Arg1 = arg1;
        }
    }

    public class ApiEventArgs<TArg1, TArg2> : ApiEventArgs
    {
        public TArg1 Arg1 { get; set; }
        public TArg2 Arg2 { get; set; }

        public ApiEventArgs()
        {
        }

        public ApiEventArgs(TArg1 arg1, TArg2 arg2)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }
    }

    public class ApiEventArgs<TArg1, TArg2, TArg3> : ApiEventArgs
    {
        public TArg1 Arg1 { get; set; }
        public TArg2 Arg2 { get; set; }
        public TArg3 Arg3 { get; set; }

        public ApiEventArgs()
        {
        }

        public ApiEventArgs(TArg1 arg1, TArg2 arg2, TArg3 arg3)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }
    }

    public class ApiEventArgs<TArg1, TArg2, TArg3, TArg4> : ApiEventArgs
    {
        public TArg1 Arg1 { get; set; }
        public TArg2 Arg2 { get; set; }
        public TArg3 Arg3 { get; set; }
        public TArg4 Arg4 { get; set; }

        public ApiEventArgs()
        {
        }

        public ApiEventArgs(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }
    }

    public class ApiEventArgs<TArg1, TArg2, TArg3, TArg4, TArg5> : ApiEventArgs
    {
        public TArg1 Arg1 { get; set; }
        public TArg2 Arg2 { get; set; }
        public TArg3 Arg3 { get; set; }
        public TArg4 Arg4 { get; set; }
        public TArg5 Arg5 { get; set; }

        public ApiEventArgs()
        {
        }

        public ApiEventArgs(TArg1 arg1, TArg2 arg2, TArg3 arg3, TArg4 arg4, TArg5 arg5)
        {
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
            Arg5 = arg5;
        }
    }
}