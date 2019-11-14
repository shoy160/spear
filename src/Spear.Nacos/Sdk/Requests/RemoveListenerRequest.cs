using System;
using System.Collections.Generic;

namespace Spear.Nacos.Sdk.Requests
{
    public class RemoveListenerRequest : ConfigRequest
    {
        /// <summary>
        /// Callbacks when configuration was changed
        /// </summary>
        /// <value>The callbacks.</value>
        public List<Action> Callbacks { get; set; } = new List<Action>();
    }
}
