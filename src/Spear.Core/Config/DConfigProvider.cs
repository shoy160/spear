using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spear.Core.Config
{
    public abstract class DConfigProvider : ConfigurationProvider
    {
        private const string ArrayPattern = @"(\[[0-9]+\])*$";
        protected readonly ILogger Logger;
        private static readonly object LockObj = new object();

        /// <summary> 是否已加载 </summary>
        private bool _hasLoaded;

        protected DConfigProvider()
        {
            Logger = CurrentIocManager.CreateLogger<DConfigProvider>();
        }

        /// <summary> 键转换 </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected internal virtual string ConvertKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return key;
            }
            var split = key.Split('.');
            var sb = new StringBuilder();
            foreach (var part in split)
            {
                var keyPart = ConvertArrayKey(part);
                sb.Append(keyPart);
                sb.Append(ConfigurationPath.KeyDelimiter);
            }

            return sb.ToString(0, sb.Length - 1);
        }

        protected internal virtual string ConvertArrayKey(string key)
        {
            return Regex.Replace(key, ArrayPattern, (match) =>
            {
                var result = match.Value.Replace("[", ":").Replace("]", string.Empty);
                return result;
            });
        }

        /// <summary> 加载配置 </summary>
        /// <param name="json"></param>
        /// <param name="clean"></param>
        protected virtual void LoadJson(string json, bool clean = false)
        {
            if (clean)
                Data.Clear();
            if (string.IsNullOrWhiteSpace(json))
                return;
            try
            {
                var t = new JsonConfigurationParser().Parse(json);
                foreach (var key in t.Keys)
                {
                    var dKey = ConvertKey(key);
                    if (string.IsNullOrWhiteSpace(dKey))
                        continue;
                    Data[dKey] = t[key];
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
            }
        }

        protected abstract Task LoadConfig(bool reload = false);

        public override void Load()
        {
            if (_hasLoaded)
                return;
            lock (LockObj)
            {
                if (_hasLoaded) return;
                _hasLoaded = true;
            }

            LoadConfig().ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
