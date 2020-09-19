using System;
using System.Collections.Generic;
using Spear.Core.Dependency;

namespace Spear.Core.EventBus
{
    public interface IConsumeConfigurator : ISingleDependency
    {
        void Configure(List<Type> consumers);
    }
}
