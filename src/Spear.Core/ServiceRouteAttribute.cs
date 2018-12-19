﻿using System;

namespace Spear.Core
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ServiceRouteAttribute : Attribute
    {
        public string Route { get; }

        public ServiceRouteAttribute(string route)
        {
            Route = route;
        }
    }
}
