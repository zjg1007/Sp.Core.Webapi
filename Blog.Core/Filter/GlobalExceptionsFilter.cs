using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blog.Core.Common.LogHelper;
using Blog.Core.Hubs;
using Blog.Core.Log;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Profiling;

namespace Blog.Core.Filter
{
    public class GlobalExceptionsFilter : IExceptionFilter
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHubContext<ChatHub> _hubContext;
        public GlobalExceptionsFilter(IWebHostEnvironment env, ILoggerHelper loggerHelper,IHubContext<ChatHub> hubContext)
        {
            _env = env;
            _loggerHelper = loggerHelper;
            _hubContext = hubContext;
        }
        public  void  OnException(ExceptionContext context)
        {
            var json = new JsonErrorResponse();
            json.Message = context.Exception.Message;//错误信息
            //返回的JSON参数
            context.HttpContext.Request.Body.Position = 0;
            var requestReader = new StreamReader(context.HttpContext.Request.Body);
            var requestContent = requestReader.ReadToEnd();
            string querystring = requestContent;
            json.ResultJson = requestContent;
            //接口路径
            string apiPath = context.HttpContext.Request.Path.Value;
            //if (_env.IsDevelopment())
            {
                json.DevelopmentMessage = context.Exception.StackTrace;//堆栈信息
            }
            context.Result = new InternalServerErrorObjectResult(json);
            MiniProfiler.Current.CustomTiming("Errors：", json.Message);
            //采用log4net 进行错误日志记录
            _loggerHelper.Error(json.Message, WriteLog(json.Message, querystring, context.Exception,apiPath));

            _hubContext.Clients.All.SendAsync("ReceiveUpdate", LogLock.GetLogData()).Wait();
        }
        /// <summary>
        /// 自定义返回格式
        /// </summary>
        /// <param name="throwMsg">自定义错误</param>
        /// <param name="resultJson">传入参数</param>
        /// <param name="ex">异常提示</param>
        /// <param name="apiPath">接口路径</param>
        /// <returns></returns>
        public string WriteLog(string throwMsg, string resultJson,Exception ex,string apiPath)
        {
            return string.Format("【异常接口路径：】{5} \r\n【自定义错误】：{0} \r\n【异常类型】：{1} \r\n【异常信息】：{2} \r\n【堆栈调用】：{3}\r\n【传入参数Json：】{4} \r\n", new object[] { throwMsg,
                ex.GetType().Name, ex.Message, ex.StackTrace,resultJson ,apiPath});
        }

    }
    public class InternalServerErrorObjectResult : ObjectResult
    {
        public InternalServerErrorObjectResult(object value) : base(value)
        {
            StatusCode = StatusCodes.Status500InternalServerError;
        }
    }
    //返回错误信息
    public class JsonErrorResponse
    {
        /// <summary>
        /// 生产环境的消息
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 开发环境的消息
        /// </summary>
        public string DevelopmentMessage { get; set; }
        /// <summary>
        /// 传入Json参数
        /// </summary>
        public string ResultJson { get; set; }
    }

}
