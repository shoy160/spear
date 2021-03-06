﻿using Spear.Core.Attributes;
using Spear.Core.Config;
using Spear.Core.Message.Implementation;
using Spear.Core.Message.Json.Models;

namespace Spear.Core.Message.Json
{
    [Codec(ServiceCodec.Json)]
    public class JsonCodec : DMessageCodec<JsonDynamic, JsonInvokeMessage, JsonResultMessage>
    {
        public JsonCodec(IMessageSerializer serializer, SpearConfig config = null) : base(serializer, config)
        {
        }
    }
}
