using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GitHub.Discord.Bot.Commons.Exceptions
{
    [Serializable]
    public class InvalidReportException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //
        
        public List<string> MissingReportContext { get; set; }

        public InvalidReportException(string message) : base(message)
        {
            MissingReportContext = new List<string>();
        }
        public InvalidReportException(List<string> missingReportContext)
        {
            MissingReportContext = missingReportContext;
        }

        public InvalidReportException(string message, List<string> missingReportContext) : base(message)
        {
            MissingReportContext = missingReportContext;
        }

        public InvalidReportException(string message, List<string> missingReportContext, Exception inner) : base(message, inner)
        {
            MissingReportContext = missingReportContext;
        }

        protected InvalidReportException(
            List<string> missingReportContext,
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
            MissingReportContext = missingReportContext;
        }
    }
}