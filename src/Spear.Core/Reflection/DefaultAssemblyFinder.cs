using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Spear.Core.Reflection
{
    public class DefaultAssemblyFinder : IAssemblyFinder
    {
        private readonly Func<Assembly, bool> _defaultPredicate;
        private readonly Func<string, bool> _dllPredicate;

        public DefaultAssemblyFinder(Func<string, bool> dllPredicate = null)
        {
            _defaultPredicate = AssemblyFilter;
            _dllPredicate = dllPredicate ?? (t => true);
        }

        private static readonly Func<Assembly, bool> AssemblyFilter = a =>
        {
            if (a.FullName.StartsWith("Microsoft.") || a.FullName.StartsWith("System.") ||
                a.FullName.StartsWith("DotNetty."))
                return false;
            if (new[] { "Polly", "Consul", "Newtonsoft.Json" }.Contains(a.GetName().Name))
                return false;
            return true;
        };

        private static IEnumerable<Assembly> _allAssemblies;

        /// <summary> 查找所有程序集 </summary>
        /// <returns></returns>
        public IEnumerable<Assembly> FindAll()
        {
            if (_allAssemblies != null && _allAssemblies.Any())
                return _allAssemblies;
            var asses = new List<Assembly>();
            var dps = DependencyContext.Default;
            var libs = dps.CompileLibraries.Where(t => _dllPredicate.Invoke(t.Name));

            foreach (var lib in libs)
            {
                try
                {
                    var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
                    asses.Add(assembly);
                }
                catch { }
            }

            var path = AppDomain.CurrentDomain.RelativeSearchPath;
            if (!Directory.Exists(path))
                path = AppDomain.CurrentDomain.BaseDirectory;

            var dllAsses =
                Directory.GetFiles(path, "*.dll")
                    .Where(p =>
                    {
                        var fileName = Path.GetFileName(p);
                        return _dllPredicate?.Invoke(fileName) ?? true;
                    })
                    .Select(Assembly.LoadFrom)
                    .ToArray();
            asses = asses.Union(dllAsses).ToList();

            return _allAssemblies = _defaultPredicate != null ? asses.Where(_defaultPredicate) : asses;
        }

        /// <summary> 查找程序集 </summary>
        /// <param name="assemblyFunc"></param>
        /// <returns></returns>
        public IEnumerable<Assembly> Find(Func<Assembly, bool> assemblyFunc = null)
        {
            var list = FindAll();
            if (assemblyFunc != null)
                list = list.Where(assemblyFunc);
            return list;
        }
    }
}
