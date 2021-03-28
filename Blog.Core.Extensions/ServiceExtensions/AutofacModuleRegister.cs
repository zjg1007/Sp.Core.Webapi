using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using Blog.Core.AOP;
using Blog.Core.Common;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Blog.Core.Extensions.ServiceExtensions
{
   public   class AutofacModuleRegister : Autofac.Module
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AutofacModuleRegister));
        protected override void Load(ContainerBuilder builder)
        {

            var basePath = AppContext.BaseDirectory;

            #region Service.dll 注入，有对应接口
            try
            {
                //注册要通过反射创建的组件
                //builder.RegisterType<BlogCacheAOP>();//可以直接替换其他拦截器
                //builder.RegisterType<BlogRedisCacheAOP>();//可以直接替换其他拦截器
                //builder.RegisterType<BlogLogAOP>();//可以直接替换其他拦截器！



                //var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath;//获取项目路径
                //var servicesDllFile = Path.Combine(basePath, "Blog.Core.Services.dll");
                //var assemblysServices = Assembly.LoadFrom(servicesDllFile);//直接采用加载文件的方法  ※※★※※ 如果你是第一次下载项目，请先F6编译，然后再F5执行，※※★※※

                var servicesDllFile = Path.Combine(basePath, "Blog.Core.Services.dll");
                var repositoryDllFile = Path.Combine(basePath, "Blog.Core.Repository.dll");

                if (!(File.Exists(servicesDllFile) && File.Exists(repositoryDllFile)))
                {
                    var msg = "Repository.dll和service.dll 丢失，因为项目解耦了，所以需要先F6编译，再F5运行，请检查 bin 文件夹，并拷贝。";
                    log.Error(msg);
                    throw new Exception(msg);
                }
                //var assemblysServices = Assembly.LoadFrom(servicesDllFile);
                //builder.RegisterAssemblyTypes(assemblysServices).AsImplementedInterfaces();

                // AOP 开关，如果想要打开指定的功能，只需要在 appsettigns.json 对应对应 true 就行。
                var cacheType = new List<Type>();
                if (Appsettings.app(new string[] { "AppSettings", "RedisCachingAOP", "Enabled" }).ObjToBool())
                {
                    builder.RegisterType<BlogRedisCacheAOP>();//可以直接替换其他拦截器
                    cacheType.Add(typeof(BlogRedisCacheAOP));
                }
                if (Appsettings.app(new string[] { "AppSettings", "MemoryCachingAOP", "Enabled" }).ObjToBool())
                {
                    builder.RegisterType<BlogCacheAOP>();//可以直接替换其他拦截器
                    cacheType.Add(typeof(BlogCacheAOP));
                }
                if (Appsettings.app(new string[] { "AppSettings", "LogAOP", "Enabled" }).ObjToBool())
                {
                    builder.RegisterType<BlogLogAOP>();//可以直接替换其他拦截器！
                    cacheType.Add(typeof(BlogLogAOP));
                }
                if (Appsettings.app(new string[] { "AppSettings", "TranAOP", "Enabled" }).ObjToBool())
                {
                    builder.RegisterType<BlogTranAOP>();
                    cacheType.Add(typeof(BlogTranAOP));
                }
                // 获取 Service.dll 程序集服务，并注册
                var assemblysServices = Assembly.LoadFrom(servicesDllFile);
                builder.RegisterAssemblyTypes(assemblysServices)
                          .AsImplementedInterfaces()
                          .InstancePerDependency()
                          .EnableInterfaceInterceptors()//引用Autofac.Extras.DynamicProxy;
                          .InterceptedBy(cacheType.ToArray());//允许将拦截器服务的列表分配给注册。

                // 获取 Repository.dll 程序集服务，并注册
                var assemblysRepository = Assembly.LoadFrom(repositoryDllFile);
                builder.RegisterAssemblyTypes(assemblysRepository)
                       .AsImplementedInterfaces()
                       .InstancePerDependency();

                #region 没有接口的单独类 class 注入
                ////只能注入该类中的虚方法
                //builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Love)))
                //    .EnableClassInterceptors()
                //    .InterceptedBy(typeof(BlogLogAOP));

                #endregion

            }
            catch (Exception ex)
            {
                throw new Exception("※※★※※ 如果你是第一次下载项目，请先对整个解决方案dotnet build（F6编译），然后再对api层 dotnet run（F5执行），\n因为解耦了，如果你是发布的模式，请检查bin文件夹是否存在Repository.dll和service.dll ※※★※※" + ex.Message + "\n" + ex.InnerException);
            }

            #endregion
   


            #region 没有接口层的服务层注入

            //因为没有接口层，所以不能实现解耦，只能用 Load 方法。
            //注意如果使用没有接口的服务，并想对其使用 AOP 拦截，就必须设置为虚方法
            //var assemblysServicesNoInterfaces = Assembly.Load("Blog.Core.Services");
            //builder.RegisterAssemblyTypes(assemblysServicesNoInterfaces);

            #endregion

            #region 没有接口的单独类，启用class代理拦截

            //只能注入该类中的虚方法，且必须是public
            //这里仅仅是一个单独类无接口测试，不用过多追问
            //builder.RegisterAssemblyTypes(Assembly.GetAssembly(typeof(Love)))
            //    .EnableClassInterceptors()
            //    .InterceptedBy(cacheType.ToArray());
            #endregion

            #region 单独注册一个含有接口的类，启用interface代理拦截

            //不用虚方法
            //builder.RegisterType<AopService>().As<IAopService>()
            //   .AsImplementedInterfaces()
            //   .EnableInterfaceInterceptors()
            //   .InterceptedBy(typeof(BlogCacheAOP));
            #endregion


        }
    }
}
