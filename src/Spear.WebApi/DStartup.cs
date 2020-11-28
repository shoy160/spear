using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using NJsonSchema;
using NSwag;
using NSwag.AspNetCore;
using NSwag.Generation.AspNetCore;
using Spear.Core.Config;
using Spear.Core.Serialize;
using Spear.Core.Timing;
using Spear.Core.Extensions;
using Spear.Framework;
using Spear.WebApi.Filters;
using Spear.WebApi.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Spear.Core;

namespace Spear.WebApi
{
    public class DStartup
    {
        /// <summary> 启动类 </summary>
        protected readonly SpearBootstrap Bootstrap;
        private readonly DocumentType _docType;
        private readonly IDictionary<string, string> _docGroups;

        private const string DefaultApiKey = "api";

        private const string SwaggerRoute = "/swagger/{0}/swagger.json";

        /// <summary> 构造函数 </summary>
        /// <param name="name">默认文档名称</param>
        /// <param name="docType">API文档类型</param>
        protected DStartup(string name = null, DocumentType docType = DocumentType.SwaggerUi)
        {
            Bootstrap = new SpearBootstrap();
            _docType = docType;
            _docGroups = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(name))
                _docGroups.Add(DefaultApiKey, name);
        }

        private bool EnableSwagger => "swagger".Config(false) || Constants.Mode != ProductMode.Prod;

        #region Swagger

        /// <summary> 接口分组 </summary>
        /// <returns></returns>
        protected virtual IDictionary<string, string> DocGroups()
        {
            return new Dictionary<string, string>();
        }

        protected virtual void OpenApiSetting(OpenApiDocumentMiddlewareSettings options)
        {
        }

        protected virtual void SwaggerDocumentSetting(AspNetCoreOpenApiDocumentGeneratorSettings options)
        {
        }

        protected virtual void ReDocUiSetting(ReDocSettings setting)
        {
        }

        protected virtual void SwaggerUiSetting(SwaggerUiSettings setting)
        {
        }

        protected virtual void SwaggerUi3Setting(SwaggerUi3Settings options)
        {
            options.WithCredentials = true;
        }

        private void AddSwagger(IServiceCollection services)
        {
            var groups = DocGroups();
            if (groups.Any())
            {
                foreach (var (key, title) in groups)
                {
                    if (_docGroups.ContainsKey(key))
                        _docGroups[key] = title;
                    else
                        _docGroups.TryAdd(key, title);
                }
            }
            if (!_docGroups.Any())
            {
                var assName = Assembly.GetExecutingAssembly().GetName();
                _docGroups.Add(DefaultApiKey, assName.Name);
            }

            foreach (var (key, value) in _docGroups)
            {
                services.AddSwaggerDocument(options =>
                {
                    options.SchemaType = _docType == DocumentType.SwaggerUi ? SchemaType.Swagger2 : SchemaType.OpenApi3;
                    options.DocumentName = key;
                    options.Title = value;
                    if (key != DefaultApiKey)
                    {
                        options.ApiGroupNames = new[] { key };
                    }

                    options.AddSecurity("spear", new OpenApiSecurityScheme
                    {
                        Type = OpenApiSecuritySchemeType.ApiKey,
                        Description = "授权(数据将在请求头中进行传输) 参数结构: \"Authorization: spear {token}\"",
                        Name = "Authorization", //OAuth2默认的参数名称
                        In = OpenApiSecurityApiKeyLocation.Header,
                        Scheme = "spear"
                    });
                    SwaggerDocumentSetting(options);
                });
            }
        }

        private void UseSwagger(IApplicationBuilder app)
        {
            app.UseOpenApi(OpenApiSetting);

            switch (_docType)
            {
                case DocumentType.SwaggerUi:

                    app.UseFileServer(new FileServerOptions
                    {
                        RequestPath = new PathString("/swagger/spear"),
                        FileProvider = new EmbeddedFileProvider(typeof(DStartup).GetTypeInfo().Assembly, "Spear.WebApi.Content")
                    });

                    foreach (var doc in _docGroups)
                    {
                        app.UseSwaggerUi(setting =>
                        {
                            setting.DocumentPath = string.Format(SwaggerRoute, doc.Key);
                            setting.CustomJavaScriptPath = "/swagger/spear/swagger.js";
                            setting.CustomStylesheetPath = "/swagger/spear/swagger.css";
                            SwaggerUiSetting(setting);
                        });
                    }

                    break;
                case DocumentType.ReDoc:
                    foreach (var docGroup in _docGroups)
                    {
                        app.UseReDoc(setting =>
                        {
                            setting.DocumentPath = string.Format(SwaggerRoute, docGroup.Key);
                            ReDocUiSetting(setting);
                        });
                    }

                    break;
                default:
                    app.UseSwaggerUi3(options =>
                    {
                        foreach (var (key, value) in _docGroups)
                        {
                            options.SwaggerRoutes.Add(new SwaggerUi3Route(value, string.Format(SwaggerRoute, key)));
                        }

                        SwaggerUi3Setting(options);
                    });
                    break;
            }
        }



        #endregion

        protected virtual void MapServices(ContainerBuilder builder)
        {
        }

        protected virtual void MapServices(IServiceCollection services)
        {

        }

        protected virtual void UseServices(IServiceProvider provider)
        {
        }

        protected virtual void ConfigJsonOptions(MvcNewtonsoftJsonOptions options)
        {

        }

        protected virtual void ConfigMvcOptions(MvcOptions options)
        {

        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
            if (EnableSwagger)
                AddSwagger(services);

            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services
                .AddControllers(options =>
                {
                    options.Filters.Add<DExceptionFilter>();

                    //fix enum validate Exception for undefined
                    //https://github.com/dotnet/aspnetcore/blob/master/src/Mvc/Mvc.Core/src/ModelBinding/Binders/EnumTypeModelBinder.cs
                    options.ModelBinderProviders.Replace<EnumTypeModelBinderProvider>(
                        new SpearEnumTypeModelBinderProvider());
                    ConfigMvcOptions(options);
                })
                .AddNewtonsoftJson(options =>
                {
                    // fix null value convert Exception
                    //options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    //options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
                    options.SerializerSettings.Converters.Add(new DateTimeConverter());
                    //options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    ConfigJsonOptions(options);
                })
                .AddControllersAsServices()
                .AddPropertyIoc();
            services.AddHealthChecks();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            MapServices(services);
        }

        public virtual void ConfigureContainer(ContainerBuilder builder)
        {
            Bootstrap.Builder = builder;
            Bootstrap.BuilderHandler += MapServices;
            Bootstrap.Initialize();
        }

        protected virtual void ConfigRoute(IEndpointRouteBuilder builder)
        {
            
        }

        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var provider = app.ApplicationServices;
            var container = provider.GetAutofacRoot();
            Bootstrap.CreateContainer(container);
            if (EnableSwagger)
                UseSwagger(app);

            app.UseRouting();
            app.UseCors("default");

            app.UseEndpoints(routeBuilder =>
            {
                //普通路由
                routeBuilder.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
                ////区域路由
                //routeBuilder.MapAreaControllerRoute("areaRoute", "", "{area:exists}/{controller}/{action=Index}/{id?}");
                //健康检查
                routeBuilder.MapHealthChecks("/healthz", new HealthCheckOptions());
                //刷新配置
                routeBuilder.MapGet("/config/reload", async ctx =>
                {
                    ConfigHelper.Instance.Reload();
                    await ctx.Response.WriteAsync("ok");
                });
                if (EnableSwagger && _docType == DocumentType.SwaggerUi)
                {
                    routeBuilder.MapGet("/swagger/routes", async ctx =>
                    {
                        var routes = _docGroups.Select(t => new
                        {
                            url = string.Format(SwaggerRoute, t.Key),
                            title = t.Value
                        });
                        ctx.Response.ContentType = "application/json";
                        await ctx.Response.WriteAsync(JsonHelper.ToJson(routes), Encoding.UTF8);
                    });
                }
                ConfigRoute(routeBuilder);
            });
            UseServices(provider);
            var liftscope = provider.GetService<IHostApplicationLifetime>();
            liftscope.ApplicationStopping.Register(Bootstrap.Dispose);
        }

        protected enum DocumentType
        {
            SwaggerUi,
            SwaggerUi3,
            ReDoc
        }
    }
}
