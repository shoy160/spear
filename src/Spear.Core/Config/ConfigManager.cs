using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Spear.Core.Config
{
    /// <summary> 配置管理器 </summary>
    public class ConfigManager
    {
        private IConfigurationRoot _config;
        private const string ConfigName = "appsettings.json";
        private const string EnvConfigParttern = "^\\$\\{([^\\}]+)\\}\\s*|\\s*([\\w\\W]+)$";
        private IDisposable _callbackRegistration;
        private IConfigurationBuilder _builder;

        /// <summary> 配置文件变更事件 </summary>
        public event Action<object> ConfigChanged;

        /// <summary> 当前配置 </summary>
        public IConfiguration Config => _config;

        private ConfigManager()
        {
            InitBuilder();
            InitConfig();
        }

        /// <summary> 单例 </summary>
        public static ConfigManager Instance => Singleton<ConfigManager>.Instance ??
                                                (Singleton<ConfigManager>.Instance = new ConfigManager());

        private void InitBuilder()
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            _builder = new ConfigurationBuilder().SetBasePath(currentDir);
            var path = Path.Combine(currentDir, ConfigName);
            if (File.Exists(path))
            {
                _builder.AddJsonFile(ConfigName, false, true);
            }
        }

        private void InitConfig()
        {
            _config = _builder.Build();
            _callbackRegistration = _config.GetReloadToken().RegisterChangeCallback(OnConfigChanged, _config);
        }

        private void OnConfigChanged(object state)
        {
            ConfigChanged?.Invoke(state);
            _callbackRegistration?.Dispose();
            _callbackRegistration = _config.GetReloadToken().RegisterChangeCallback(OnConfigChanged, state);
        }

        /// <summary> 构建配置 </summary>
        /// <param name="builderAction"></param>
        public void Build(Action<IConfigurationBuilder> builderAction)
        {
            builderAction.Invoke(_builder);
            var sources = _builder.Sources.Reverse().ToArray();
            //倒序排列，解决读取配置时的优先级问题
            for (var i = 0; i < sources.Length; i++)
            {
                _builder.Sources[i] = sources[i];
            }

            _config = _builder.Build();
        }

        private T GetEnvOrValue<T>(string value, T defaultValue)
        {
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            var match = Regex.Match(value, EnvConfigParttern);
            if (!match.Success)
                return value.CastTo(defaultValue);
            //获取环境变量
            var env = match.Groups[1].Value.Env(defaultValue);
            if (env != null && !env.Equals(defaultValue))
                return env;
            return match.Groups[2].Value.CastTo(defaultValue);
        }

        /// <summary> 配置文件读取 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">默认值</param>
        /// <param name="key">配置名</param>
        /// <returns></returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            var type = typeof(T);
            if (type.IsSimpleType() || type.IsEnum)
            {
                var str = _config.GetValue<string>(key);
                return GetEnvOrValue(str, defaultValue);
            }

            try
            {
                //区分大小写
                return _config.GetSection(key).Get<T>();
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary> 配置文件读取 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="defaultValue">默认值</param>
        /// <param name="key">配置名</param>
        /// <param name="supressKey">配置别名</param>
        /// <returns></returns>
        public T GetConfig<T>(T defaultValue = default, [CallerMemberName] string key = null,
              string supressKey = null)
        {
            if (!string.IsNullOrWhiteSpace(supressKey))
                key = supressKey;
            return Get(key, defaultValue);
        }

        /// <summary> 重新加载配置 </summary>
        public void Reload() { _config.Reload(); }


    }
}
