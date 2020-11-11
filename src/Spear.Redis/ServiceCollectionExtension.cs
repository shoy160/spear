using System;

namespace Spear.Redis
{
    public static class ServiceCollectionExtension
    {
        /// <summary> 开启两级缓存同步 </summary>
        /// <param name="provider"></param>
        /// <param name="configName"></param>
        /// <returns></returns>
        public static IServiceProvider UseCacheSync(this IServiceProvider provider, string configName = null)
        {
            //订阅
            RedisManager.Instance.CacheSubscribe(configName);
            return provider;
        }
    }
}
