
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Blog.Core.Common
{
    /// <summary>
    /// appsettings.json操作类
    /// </summary>
    public class Appsettings
    {
        static IConfiguration Configuration { get; set; }
        static string contentPath { get; set; }

        //public Appsettings(IHostingEnvironment env)
        //{
        //    contentPath = env.ContentRootPath;
        //}

        //static Appsettings()
        //{
        //    //ReloadOnChange = true 当appsettings.json被修改时重新加载
        //    Configuration = new ConfigurationBuilder()
        //    .Add(new JsonConfigurationSource { Path = "appsettings.json", ReloadOnChange = true })//请注意要把当前appsetting.json 文件->右键->属性->复制到输出目录->始终复制
        //    .Build();
        //}
        public Appsettings(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public Appsettings(string contentPath)
        {
            string Path = "appsettings.json";

            {
                //如果你把配置文件 是 根据环境变量来分开了，可以这样写
                //Path = $"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json";
            }

            //Configuration = new ConfigurationBuilder()
            //.Add(new JsonConfigurationSource { Path = Path, ReloadOnChange = true })//请注意要把当前appsetting.json 文件->右键->属性->复制到输出目录->始终复制
            //.Build();


            //var contentPath = env.ContentRootPath;
            Configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .Add(new JsonConfigurationSource { Path = Path, Optional = false, ReloadOnChange = true })//这样的话，可以直接读目录里的json文件，而不是 bin 文件夹下的，所以不用修改复制属性
               .Build();


        }

        /// <summary>
        /// 封装要操作的字符
        /// </summary>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static string app(params string[] sections)
        {
            try
            {

                if (sections.Any())
                {
                    return Configuration[string.Join(":", sections)];
                }
            }
            catch (Exception) { }

            return "";

        }
        /// <summary>
        /// 递归获取配置信息数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sections"></param>
        /// <returns></returns>
        public static List<T> app<T>(params string[] sections)
        {
            List<T> list = new List<T>();
            // 引用 Microsoft.Extensions.Configuration.Binder 包
            Configuration.Bind(string.Join(":", sections), list);
            return list;
        }
    }
}
