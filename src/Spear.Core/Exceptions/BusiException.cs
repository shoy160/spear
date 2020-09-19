using System;

namespace Spear.Core.Exceptions
{
    public class BusiException : Exception
    {
        public int Code { get; set; }

        public BusiException(string message, int code = 500) : base(message)
        {
            Code = code;
        }

        public BusiException(SpearException ex) : this(ex.Message, ex.Code)
        {
        }
    }
}
