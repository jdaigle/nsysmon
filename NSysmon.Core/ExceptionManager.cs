using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NSysmon.Core
{
    /// <remarks>
    /// This code is derived from https://github.com/opserver/Opserver/tree/a170ea8bcda9f9e52d4aaff7339f3d198309369b
    /// under "The MIT License (MIT)". Copyright (c) 2013 Stack Exchange Inc.
    /// </remarks>
    public static class ExceptionManager
    {
        public static void LogException(string message, Exception innerException)
        {
            var ex = new Exception(message, innerException);
            LogException(ex);
        }

        public static void LogException(Exception ex)
        {
            // TODO:
            //ErrorStore.LogExceptionWithoutContext(ex, appendFullStackTrace: true);
        }
    }
}
