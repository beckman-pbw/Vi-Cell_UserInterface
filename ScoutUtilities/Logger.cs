using System;
using ScoutUtilities.Enums;
using System.Runtime.CompilerServices;

namespace ScoutUtilities
{
    public static class Logger
    {
        public static string GetHawkeyeErrorMessage(HawkeyeError error, [CallerFilePath] string file = "UNKNOWN-FILE", [CallerLineNumber] int lineNumber = -1,
            [CallerMemberName] string member = "UNKNOWN-CALL-SITE")
        {
            if (error != HawkeyeError.eSuccess)
            {
                return $"HawkeyeError found:\n\tFile: '{file}' (Line: {lineNumber}),\n\tMethod or Property: '{member}':\n\tError: '{error.ToString()}'";
            }

            return string.Empty;
        }

        public static string GetAllExceptionMessages(Exception exception, int depth = 0)
        {
            if (exception == null)
                return string.Empty;

            var tabs = string.Empty;
            for (var i = 0; i < depth; i++)
            {
                tabs += "\t";
            }

            var str = tabs + exception.Message;
            if (exception.InnerException != null)
            {
                return str + Environment.NewLine + GetAllExceptionMessages(exception.InnerException, depth + 1);
            }

            return str;
        }
    }
}
