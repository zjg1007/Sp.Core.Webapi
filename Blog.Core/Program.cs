using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Blog.Core.Model.See;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Blog.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //初始化默认主机Builder
            Host.CreateDefaultBuilder(args)
             .UseServiceProviderFactory(new AutofacServiceProviderFactory())
             .ConfigureWebHostDefaults(webBuilder =>
             {
                 webBuilder
                 .UseStartup<Startup>()
                 .UseUrls("http://*:5001")
                 .ConfigureLogging((hostingContext, builder) =>
                 {
                     //过滤掉系统默认的一些日志
                     builder.AddFilter("System", LogLevel.Error);
                     builder.AddFilter("Microsoft", LogLevel.Error);
                     builder.AddFilter("Blog.Core.AuthHelper.ApiResponseHandler", LogLevel.Error);
                     //Console.WriteLine("日志文件目录"+Directory.GetCurrentDirectory());
                     //可配置文件
                     var path = Path.Combine(Directory.GetCurrentDirectory(), "Log4net.config");
                     builder.AddLog4Net(path);
                 });
             })
            // 生成承载 web 应用程序的 Microsoft.AspNetCore.Hosting.IWebHost。Build是WebHostBuilder最终的目的，将返回一个构造的WebHost，最终生成宿主。
             .Build()
            // 运行 web 应用程序并阻止调用线程, 直到主机关闭。
            // ※※※※ 有异常，查看 Log 文件夹下的异常日志 ※※※※  
             .Run();
        }

    }

}
