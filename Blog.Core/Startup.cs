using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Blog.Core.AOP;
using Blog.Core.AuthHelper;
using Blog.Core.AuthHelper.OverWrite;
using Blog.Core.Common;
using Blog.Core.Common.MemoryCache;
using Blog.Core.FrameWork.IRepository;
using Blog.Core.Hubs;
using Blog.Core.IRepository;
using Blog.Core.IServices;
using Blog.Core.Model;
using Blog.Core.Log;
using Blog.Core.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;
using AutoMapper;
using log4net.Repository;
using log4net;
using log4net.Config;
using Blog.Core.Filter;
using StackExchange.Profiling.Storage;
using Blog.Core.Common.DB;
using Blog.Core.Common.HttpContextUser;
using Blog.Core.Extensions;
using Blog.Core.Extensions.ServiceExtensions;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Blog.Core.AuthHelper.Policys;
using Microsoft.AspNetCore.Authentication;
using Newtonsoft.Json.Serialization;
using Blog.Core.Common.LogHelper;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Blog.Core.Model.See;

namespace Blog.Core
{
    public class Startup
    {
        /// <summary>
        /// log4net 仓储库
        /// </summary>
        public static ILoggerRepository repository { get; set; }
        private IServiceCollection _services;

        private static readonly ILog log = LogManager.GetLogger(typeof(Startup));
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
            //log4net
            repository = LogManager.CreateRepository("Blog.Core");//需要获取日志的仓库名，也就是你的当然项目名

            //指定配置文件，如果这里你遇到问题，应该是使用了InProcess模式，请查看Blog.Core.csproj,并删之 
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));//配置文件
        }
        public IWebHostEnvironment Env { get; }
        private const string ApiName = "App.Core";
        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            #region 部分服务注入-netcore自带方法
       

            services.AddSingleton(new Appsettings(Configuration));
            services.AddSingleton(new LogLock(Env.ContentRootPath));
            services.AddSingleton<ILoggerHelper, LogHelper>();
            Permissions.IsUseIds4 = Appsettings.app(new string[] { "Startup", "IdentityServer4", "Enabled" }).ObjToBool();
            // Redis注入
            services.AddRedisCacheSetup();
            services.AddRedisInitMqSetup();//redis 消息队列
            services.AddMemoryCacheSetup();
            services.AddSqlsugarSetup();
            services.AddDbSetup();
            services.AddAutoMapperSetup();
            services.AddCorsSetup();
            services.AddMiniProfilerSetup();
            services.AddSwaggerSetup();
            services.AddHttpContextSetup();
            services.AddAppConfigSetup();

            services.AddRabbitMQSetup();
            services.AddEventBusSetup();
            #endregion
            // 授权+认证 (jwt or ids4)
            services.AddAuthorizationSetup();
            if (Permissions.IsUseIds4)
            {
                services.AddAuthentication_Ids4Setup();
            }
            else
            {
                services.AddAuthentication_JWTSetup();
            }
            services.AddIpPolicyRateLimitSetup(Configuration);

            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.AddScoped<UseServiceDIAttribute>();

            //autoface 接口注入必加,否则注入失败报错
            services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
        .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);

            services.AddControllers(o =>
            {
                // 全局异常过滤
                o.Filters.Add(typeof(GlobalExceptionsFilter));
                // 全局路由权限公约
                //o.Conventions.Insert(0, new GlobalRouteAuthorizeConvention());
                // 全局路由前缀，统一修改路由
                o.Conventions.Insert(0, new GlobalRoutePrefixFilter(new RouteAttribute(RoutePrefix.Name)));
            })
                //全局配置Json序列化处理
                .AddNewtonsoftJson(options =>
                {
                    //忽略循环引用
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    //不使用驼峰样式的key
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    //设置时间格式
                    //options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
                });

            _services = services;
        }

        // 注意在Program.CreateHostBuilder，添加Autofac服务工厂
        public void ConfigureContainer(ContainerBuilder builder)
        {

            builder.RegisterModule(new AutofacModuleRegister());
            //return new AutofacServiceProvider(ApplicationContainer);//第三方IOC接管 core内置DI容器
        }


        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, MyContext myContext, IHostApplicationLifetime lifetime)
        {
            // Ip限流,尽量放管道外层
            app.UseIpLimitMildd();
            // 记录请求与返回数据 
            app.UseReuestResponseLog();
            // signalr 
            app.UseSignalRSendMildd();
            // 记录ip请求
            app.UseIPLogMildd();
            // 查看注入的所有服务
            app.UseAllServicesMildd(_services);

            if (env.IsDevelopment())
            {
                // 在开发环境中，使用异常页面，这样可以暴露错误堆栈信息，所以不要放在生产环境。
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // 在非开发环境中，使用HTTP严格安全传输(or HSTS) 对于保护web安全是非常重要的。
                // 强制实施 HTTPS 在 ASP.NET Core，配合 app.UseHttpsRedirection
                //app.UseHsts();
            }

            // 封装Swagger展示
            app.UseSwaggerMildd(() => GetType().GetTypeInfo().Assembly.GetManifestResourceStream("Blog.Core.index.html"));

            // ↓↓↓↓↓↓ 注意下边这些中间件的顺序，很重要 ↓↓↓↓↓↓

            // CORS跨域
            app.UseCors("AllRequests");
            // 跳转https
            //app.UseHttpsRedirection();
            // 使用静态文件
            app.UseStaticFiles();
            // 使用cookies
            app.UseCookiePolicy();
            // 返回错误码
            app.UseStatusCodePages();
            // Routing
            app.UseRouting();

            // 测试用户，用来通过鉴权
            if (Configuration.GetValue<bool>("AppSettings:UseLoadTest"))
            {
                app.UseByPassAuthMidd();
            }
            // 这种自定义授权中间件，可以尝试，但不推荐
            // app.UseJwtTokenAuth();
            // 先开启认证
            app.UseAuthentication();
            // 然后是授权中间件
            app.UseAuthorization();
            // 开启异常中间件，要放到最后
            //app.UseExceptionHandlerMidd();
            // 性能分析
            app.UseMiniProfiler();
            // 用户访问记录
            app.UseRecordAccessLogsMildd();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHub<ChatHub>("/api2/chatHub");
            });

            // 生成种子数据
            app.UseSeedDataMildd(myContext, Env.WebRootPath);
            // 开启QuartzNetJob调度服务
            //app.UseQuartzJobMildd(tasksQzServices, schedulerCenter);
            //服务注册
            app.UseConsulMildd(Configuration, lifetime);
            // 事件总线，订阅服务
            app.ConfigureEventBus();
        }
    }
}
