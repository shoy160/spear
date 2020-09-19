using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Spear.Core.Dependency;
using Spear.Core.Helper;
using Spear.Core.Serialize;
using Spear.Core.Session;
using Spear.Core.Timing;
using Spear.Core.Extensions;
using System;
using System.Net.Http.Headers;

namespace Spear.WebApi
{
    /// <summary> 客户端令牌接口 </summary>
    public interface IClientTicket
    {
        /// <summary> 用户Id </summary>
        object UserId { get; set; }
        /// <summary> 租户Id </summary>
        object TenantId { get; set; }

        /// <summary> 用户名称 </summary>
        string Name { get; set; }

        /// <summary> 用户角色 </summary>
        string Role { get; set; }

        /// <summary> 令牌 </summary>
        string Ticket { get; set; }

        /// <summary> 过期时间 </summary>
        DateTime? ExpiredTime { get; set; }

        /// <summary> 时间戳 </summary>
        DateTime Timestamp { get; set; }
    }

    public class ClientTicket : IClientTicket
    {
        public object UserId { get; set; }
        public object TenantId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }

        public string Ticket { get; set; }

        /// <inheritdoc />
        /// <summary> 过期时间 </summary>
        public DateTime? ExpiredTime { get; set; }

        private DateTime _timestamp;

        /// <inheritdoc />
        /// <summary> 时间戳 </summary>
        public DateTime Timestamp
        {
            get => _timestamp == DateTime.MinValue ? Clock.Now : _timestamp;
            set => _timestamp = value;
        }

        /// <summary> 生成令牌 </summary>
        /// <returns></returns>
        public virtual string GenerateTicket()
        {
            return JsonHelper.ToJson(this).Md5();
        }
    }

    /// <inheritdoc />
    /// <summary> 默认客户端令牌 </summary>
    public class DClientTicket<T> : ClientTicket
    {
        private T _id;
        /// <summary> 用户ID </summary>
        public T Id
        {
            get => _id;
            set
            {
                UserId = value;
                _id = value;
            }
        }
    }

    /// <inheritdoc />
    /// <summary>
    /// 默认客户端令牌
    /// </summary>
    public class DClientTicket : DClientTicket<Guid>
    {
    }

    /// <summary> 客户端密钥辅助类 </summary>
    public static class ClientTicketHelper
    {
        private const string TicketEncodeKey = "@%d^#41&";
        private const string TicketEncodeIv = "%@D^d$2~";

        /// <summary> 获取凭证 </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static string Ticket(this ClientTicket ticket)
        {
            ticket.Ticket = ticket.GenerateTicket();

            var json = JsonHelper.ToJson(ticket);
            return EncryptHelper.SymmetricEncrypt($"{ticket.Ticket}_{json}", EncryptHelper.SymmetricFormat.DES,
                TicketEncodeKey, TicketEncodeIv);
        }

        /// <summary> 设置Session </summary>
        /// <param name="client"></param>
        public static void SetSession(this IClientTicket client)
        {
            if (client == null) return;
            var principal = CurrentIocManager.Resolve<IPrincipalAccessor>();
            principal?.SetSession(new SessionDto(client.UserId, client.TenantId)
            {
                UserName = client.Name,
                Role = client.Role
            });
        }

        /// <summary> 获取凭证信息(设置session) </summary>
        /// <param name="ticket"></param>
        /// <returns></returns>
        public static TTicket Client<TTicket>(this string ticket) where TTicket : IClientTicket
        {
            if (string.IsNullOrWhiteSpace(ticket))
                return default;
            var logger = CurrentIocManager.CreateLogger(typeof(ClientTicketHelper));
            try
            {
                var str = EncryptHelper.SymmetricDecrypt(ticket, EncryptHelper.SymmetricFormat.DES, TicketEncodeKey,
                    TicketEncodeIv);
                if (string.IsNullOrWhiteSpace(str))
                    return default;
                var list = str.Split('_');
                //fixed json字符串中包含下划线时报错
                if (list.Length < 2)
                    return default;
                var json = str.Substring(list[0].Length + 1);
                var client = JsonHelper.Json<TTicket>(json);
                if (!string.Equals(list[0], client.Ticket, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (logger.IsEnabled(LogLevel.Warning))
                        logger.LogWarning($"client ticket not equal,{client.Ticket}:{list[0]}");
                    return default;
                }
                client.SetSession();
                return client;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"凭证解析异常：{ticket},{ex.Message}");
                return default;
            }
        }

        /// <summary> 验证令牌 </summary>
        /// <param name="request"></param>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public static TTicket VerifyTicket<TTicket>(this HttpRequest request, string scheme = "spear")
            where TTicket : IClientTicket
        {
            if (!request.Headers.TryGetValue("Authorization", out var authorize) ||
                string.IsNullOrWhiteSpace(authorize))
                return default;
            var arr = authorize.ToString().Split(' ');
            if (arr.Length != 2)
                return default;
            return new AuthenticationHeaderValue(arr[0], arr[1]).VerifyTicket<TTicket>(scheme);
        }

        /// <summary> 验证令牌 </summary>
        /// <param name="authorize"></param>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public static TTicket VerifyTicket<TTicket>(this AuthenticationHeaderValue authorize, string scheme = "spear")
            where TTicket : IClientTicket
        {
            if (authorize == null ||
                !string.Equals(authorize.Scheme, scheme, StringComparison.CurrentCultureIgnoreCase))
                return default;
            var ticket = authorize.Parameter;
            return ticket.Client<TTicket>();
        }
    }
}
