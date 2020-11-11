using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using Spear.Core.Extensions;

namespace Spear.Redis
{
    /// <summary> Redis扩展 </summary>
    public static class RedisExtensions
    {
        #region 私有方法

        /// <summary> 序列化对象 </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private static RedisValue Serialize(object obj)
        {
            if (obj == null)
            {
                return RedisValue.Null;
            }
            var type = obj.GetType();
            return type.IsSimpleType() ? obj.ToString() : JsonConvert.SerializeObject(obj);
        }

        /// <summary> 反序列化对象 </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static object Deserialize(RedisValue value, Type type)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (typeof(string) == type)
                return value.ToString();
            return type.IsSimpleType() ? value.ToString().CastTo(type) : JsonConvert.DeserializeObject(value, type);
        }

        private static T Deserialize<T>(RedisValue value)
        {
            var obj = Deserialize(value, typeof(T));
            return obj == null ? default(T) : (T)obj;
        }

        #endregion

        /// <summary> 获取缓存 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public static T Get<T>(this IDatabase database, string key)
        {
            var value = database.StringGet(key);
            return Deserialize<T>(value);
        }

        /// <summary> 获取缓存 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public static async Task<T> GetAsync<T>(this IDatabase database, string key)
        {
            var value = await database.StringGetAsync(key);
            return Deserialize<T>(value);
        }

        /// <summary> 获取缓存 </summary>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object Get(this IDatabase database, string key, Type type)
        {
            var val = database.StringGet(key);
            return Deserialize(val, type);
        }

        /// <summary> 获取缓存 </summary>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <returns></returns>
        public static async Task<object> GetAsync(this IDatabase database, string key)
        {
            return await database.GetAsync<object>(key);
        }

        /// <summary> 设置缓存 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expired">过期时间</param>
        public static void Set<T>(this IDatabase database, string key, T value, TimeSpan? expired = null)
        {
            database.StringSet(key, Serialize(value), expired);
        }

        /// <summary> 设置缓存 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <param name="expired">过期时间</param>
        public static async Task SetAsync<T>(this IDatabase database, string key, T value, TimeSpan? expired = null)
        {
            await database.StringSetAsync(key, Serialize(value), expired);
        }

        /// <summary> 批量删除 </summary>
        /// <param name="database"></param>
        /// <param name="pattern"></param>
        /// <param name="batch">批次</param>
        /// <returns></returns>
        public static async Task BatchDeleteAsync(this IDatabase database, string pattern, int batch = 5000)
        {
            await database.ScriptEvaluateAsync(LuaScript.Prepare(
                " local ks = redis.call('KEYS', @keypattern) " +
                " for i=1,#ks,@batch do " +
                "     redis.call('del', unpack(ks, i, math.min(i+@batch-1, #ks))) " + //Lua集合索引值从1为起始，unpack为解包，获取ks集合中的数据，每次5000，然后执行删除
                " end " +
                " return true "), new
                {
                    keypattern = pattern,
                    batch
                });
        }

        /// <summary> 批量更新 </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="database"></param>
        /// <param name="pattern"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task BatchUpdateAsync<T>(this IDatabase database, string pattern, T value)
        {
            await database.ScriptEvaluateAsync(LuaScript.Prepare(
                    " local ks = redis.call('KEYS', @keypattern) " +
                    " for i=1,#ks do " +
                    "     redis.call('set', ks[i], @value) " +
                    " end " +
                    " return true "),
                new { keypattern = pattern, value = Serialize(value) });
        }

        /// <summary> 批量删除Hash集合 </summary>
        /// <param name="database"></param>
        /// <param name="hashId"></param>
        /// <param name="pattern">为可使用正则表达式</param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static async Task HashBatchDeleteAsync(this IDatabase database, string hashId, string pattern, int batch = 5000)
        {
            await database.ScriptEvaluateAsync(LuaScript.Prepare(
                    " local ks = redis.call('hkeys', @hashid) " +
                    " local fkeys = {} " +
                    " for i=1,#ks do " +
                    //使用string.find进行匹配操作
                    "   if string.find(ks[i], @keypattern) then " +
                    "      fkeys[#fkeys + 1] = ks[i] " +
                    "   end " +
                    " end " +
                    " for i=1,#fkeys,@batch do " +
                    "   redis.call('hdel', @hashid, unpack(fkeys, i, math.min(i+@batch-1, #fkeys))) " +
                    " end " +
                    " return true "
                ),
                new { hashid = hashId, keypattern = pattern, batch });
        }

        /// <summary> 批量更新Hash集合 </summary>
        /// <param name="database"></param>
        /// <param name="hashId"></param>
        /// <param name="pattern">为可使用正则表达式</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static async Task HashBatchUpdateAsync<T>(this IDatabase database, string hashId, string pattern, T value)
        {
            await database.ScriptEvaluateAsync(LuaScript.Prepare(
                    " local ks = redis.call('hkeys', @hashid) " +
                    " local fkeys = {} " +
                    " for i=1,#ks do " +
                    "   if string.find(ks[i], @keypattern) then " +
                    "      fkeys[#fkeys + 1] = ks[i] " +
                    "   end " +
                    " end " +
                    " for i=1,#fkeys do " +
                    "   redis.call('hset', @hashid, fkeys[i], @value) " +
                    " end " +
                    " return true "
                ),
                new { hashid = hashId, keypattern = pattern, value = Serialize(value) });
        }

        /// <summary> 批量删除Set集合 </summary>
        /// <param name="database"></param>
        /// <param name="key"></param>
        /// <param name="pattern">可使用正则表达式</param>
        /// <param name="batch"></param>
        /// <returns></returns>
        public static async Task SetBatchDeleteAsync(this IDatabase database, string key, string pattern, int batch = 5000)
        {
            await database.ScriptEvaluateAsync(LuaScript.Prepare(
                    " local ks = redis.call('smembers', @keyid) " +
                    " local fkeys = {} " +
                    " for i=1,#ks do " +
                    "   if string.find(ks[i], @keypattern) then " +
                    "      fkeys[#fkeys + 1] = ks[i] " +
                    "   end " +
                    " end " +
                    " for i=1,#fkeys,@batch do " +
                    "   redis.call('srem', @keyid, unpack(fkeys, i, math.min(i+@batch-1, #fkeys))) " +
                    " end " +
                    " return true "
                ),
                new { keyid = key, keypattern = pattern, batch });
        }
    }
}
