using System;
using System.Runtime.Serialization;

namespace GitHub.Discord.Bot.Commons.Exceptions
{
    [Serializable]
    public class IssueCouldNotBeCreatedException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public IssueCouldNotBeCreatedException()
        {
        }

        public IssueCouldNotBeCreatedException(string message) : base(message)
        {
        }

        public IssueCouldNotBeCreatedException(string message, Exception inner) : base(message, inner)
        {
        }

        protected IssueCouldNotBeCreatedException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}