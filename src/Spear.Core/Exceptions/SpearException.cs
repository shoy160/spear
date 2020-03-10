using System;

namespace Spear.Core.Exceptions
{
    public class SpearException : Exception
    {
        public int Code { get; set; }

        public SpearException(string message, int code = 500) : base(message)
        {
            Code = code;
        }
    }
}
