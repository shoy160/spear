using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Threading.Tasks;

namespace Spear.WebApi.Filters
{
    /// <summary> 日志过滤器 </summary>
    public class RecordFilter : ActionFilterAttribute, IExceptionFilter
    {
        private const string ItemKey = "action_record_{0}";
        private readonly string _type;
        private readonly MonitorConfig _config;

        public RecordFilter(string type = null)
        {
            _type = string.IsNullOrWhiteSpace(type) ? MonitorModules.Action : type;
            _config = MonitorConfig.ResolveOrOption();
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!_config.EnableModule(_type))
            {
                await base.OnActionExecutionAsync(context, next);
                return;
            }
            var dto = new MonitorData(_type);
            var key = string.Format(ItemKey, _type);
            context.HttpContext.Items.Add(key, dto);
        }

        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (!_config.EnableModule(_type))
            {
                await next();
                return;
            }
            var manager = context.HttpContext.RequestServices.GetService<MonitorManager>();

            var key = string.Format(ItemKey, _type);
            if (manager != null && context.HttpContext.Items.TryGetValue(key, out var value) && value is MonitorData data)
            {
                var result = await next();
                data.FromHttpContext(context.HttpContext);

                if (result.Exception != null)
                {
                    data.Code = (int)HttpStatusCode.InternalServerError;
                    if (_config.Mode.HasFlag(MonitorMode.Result))
                        data.Result = result.Exception.Message;
                }
                else
                {
                    data.Code = (int)HttpStatusCode.OK;
                    if (_config.Mode.HasFlag(MonitorMode.Result))
                    {
                        switch (result.Result)
                        {
                            case ObjectResult obj:
                                data.Result = JsonConvert.SerializeObject(obj.Value);
                                break;
                            case ContentResult content:
                                data.Result = content.Content;
                                break;
                            case JsonResult json:
                                data.Result = JsonConvert.SerializeObject(json.Value);
                                break;
                            default:
                                data.Result = "success";
                                break;
                        }
                    }
                }
                data.Complete();
                manager.Record(data);
            }
            else
            {
                await next();
            }
        }

        /// <inheritdoc />
        /// <summary> 异常处理 </summary>
        /// <param name="context"></param>
        public void OnException(ExceptionContext context)
        {
            var manager = context.HttpContext.RequestServices.GetService<MonitorManager>();
            if (manager != null && context.HttpContext.Items.TryGetValue(ItemKey, out var value) &&
                value is MonitorData data)
            {
                data.Code = (int)HttpStatusCode.InternalServerError;
                data.Complete();
                if (context.Exception != null)
                {
                    if (context.Exception is BusiException busi)
                    {
                        if (busi.Code != -1)
                            data.Code = busi.Code;
                    }

                    data.Result = context.Exception.Message;
                }

                manager.Record(data);
            }
        }
    }
}
