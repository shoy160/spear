using System;
using System.Collections.Generic;
using System.Text;

namespace Spear.Core.Message.Codec
{
    public interface IMessage
    {
        string Id { get; set; }
    }
}
