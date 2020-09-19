using System;
using Spear.Core.Extensions;

namespace Spear.Core.Data.Config
{

    [Serializable]
    public class ConnectionConfig
    {
        private const string Prefix = "dapper";
        private const string DefaultConfigName = "dapperDefault";
        private const string DefaultName = "default";

        public static ConnectionConfig Config(string configName = null)
        {
            if (string.IsNullOrWhiteSpace(configName))
                configName = DefaultConfigName.Config(DefaultName);
            return $"{Prefix}:{configName}".Config<ConnectionConfig>();
        }

        /// <summary> 连接名称 </summary>
        public string Name { get; set; }

        //[XmlAttribute("is_encrypt")]
        //public bool IsEncrypt { get; set; }
        /// <summary> 数据库驱动名称 </summary>
        public string ProviderName { get; set; } = "SqlServer";
        /// <summary> 连接字符串 </summary>
        public string ConnectionString { get; set; }
    }
}
