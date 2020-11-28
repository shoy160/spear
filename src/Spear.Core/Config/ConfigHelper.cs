using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Spear.Core.Config
{
    public class ConfigHelper
    {
        private IConfigurationRoot _config;
        private const string ConfigName = "appsettings.json";
        private const string EnvConfigParttern = "^\\$\\{([^\\}]+)\\}\\s*\\|\\s*([\\w\\W]+)$";
        private IDisposable _callbackRegistration;
        private ILogger _logger => CurrentIocManager.CreateLogger<ConfigHelper>();

        public ISpearConfigBuilder Builder { get; internal set; }

        /// <summary> 配置文件变更事件 </summary>
        public event Action<object> ConfigChanged;

        /// <summary> 当前配置 </summary>
        public IConfiguration Config => _config;

        private ConfigHelper() { }

        /// <summary> 单例模式 </summary>
        public static ConfigHelper Instance => Singleton<ConfigHelper>.Instance ??
                                             (Singleton<ConfigHelper>.Instance = new ConfigHelper());

        public void SetConfig(IConfigurationRoot config)
        {
            _config = config;
            _callbackRegistration = _config.GetReloadToken().RegisterChangeCallback(OnConfigChanged, _config);
        }

        /// <summary> 添加配置 </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        public void AddProvider(IConfigurationSource source, Func<IConfigurationSource, bool> predicate = null)
        {
            predicate = predicate ?? (t => t.GetType() == source.GetType());
            var config = Builder.Sources.FirstOrDefault(predicate);
            if (config != null)
                Builder.Sources.Remove(config);
            Builder.Sources.Insert(0, source);
        }

        public void Build()
        {
            SetConfig(Builder.Build());
        }

        private void OnConfigChanged(object state)
        {
            ConfigChanged?.Invoke(state);
            _callbackRegistration?.Dispose();
            _callbackRegistration = _config.GetReloadToken().RegisterChangeCallback(OnConfigChanged, state);
        }

        internal static IConfigurationBuilder CreateDefaultBuilder()
        {
            var currentDir = Directory.GetCurrentDirectory();
            var builder = new ConfigurationBuilder().SetBasePath(currentDir);
            var path = Path.Combine(currentDir, ConfigName);
            if (File.Exists(path))
            {
                builder.AddJsonFile(ConfigName, false, true);
            }
            return builder;
        }

        public object GetEnvOrValue(string key, Type type)
        {
            if (string.IsNullOrWhiteSpace(key))
                return null;
            var match = Regex.Match(key, EnvConfigParttern);
            if (!match.Success)
                return key.CastTo(type);
            //获取环境变量
            var env = match.Groups[1].Value.Env(type);
            return env ?? match.Groups[2].Value.CastTo(type);
        }

        public T GetEnvOrValue<T>(string key, T defaultValue)
        {
            var value = GetEnvOrValue(key, typeof(T));
            if (value == null)
                return defaultValue;
            return (T)value;
        }

        public object Get(string key, Type type)
        {
            if (_config == null)
                SetConfig(CreateDefaultBuilder().Build());
            if (type.IsSimpleType() || type.IsEnum)
            {
                var str = _config.GetValue<string>(key);
                return GetEnvOrValue(str, type);
            }

            ////枚举类型处理
            //if (type.IsEnum)
            //    return _config.GetValue<string>(key).CastTo(defaultValue);
            try
            {
                //区分大小写
                return _config.GetSection(key).Get(type);
            }
            catch (Exception ex)
            {
                CurrentIocManager.CreateLogger<ConfigHelper>().LogError(ex, ex.Message);
                return null;
            }
        }

        /// <summary> 配置文件读取 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">默认值</param>
        /// <param name="key">配置名</param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            var value = Get(key, typeof(T));
            if (value == null) return defaultValue;
            return (T)value;
        }

        /// <summary> 配置文件读取 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">默认值</param>
        /// <param name="key">配置名</param>
        /// <param name="supressKey">配置别名</param>
        /// <returns></returns>
        public T Get<T>(T defaultValue = default, [CallerMemberName] string key = null,
              string supressKey = null)
        {
            if (!string.IsNullOrWhiteSpace(supressKey))
                key = supressKey;
            var value = Get(key, typeof(T));
            if (value == null) return defaultValue;
            return (T)value;
        }

        /// <summary> 重新加载配置 </summary>
        public void Reload() { _config.Reload(); }
    }
}
