using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Spear.Core.Reflection
{
    public class DefaultTypeFinder : ITypeFinder
    {
        private readonly IAssemblyFinder _assemblyFinder;

        public DefaultTypeFinder(IAssemblyFinder assemblyFinder)
        {
            _assemblyFinder = assemblyFinder;
        }

        public Type[] Find(Func<Type, bool> typeFunc = null)
        {
            typeFunc = typeFunc ?? (t => true);
            var assemblyList = _assemblyFinder.Find();
            var types = new List<Type>();
            foreach (var assembly in assemblyList)
            {
                Type[] list;

                try
                {
                    list = assembly.GetTypes().Where(typeFunc).ToArray();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    list = ex.Types.Where(typeFunc).ToArray();
                }
                if (!list.Any())
                    continue;
                types.AddRange(list.Where(t => t != null));
            }

            return types.ToArray();
        }
    }
}
