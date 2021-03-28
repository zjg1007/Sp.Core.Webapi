using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.SwaggerHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Blog.Core.Extensions.CustomApiVersion;

namespace Blog.Core.Controllers.V1
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApbController : ControllerBase
    {
        /// <summary>
        /// V1版本的测试接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [CustomRoute(ApiVersions.V1, "apbs")]
        public IEnumerable<string> Get()
        {
            return new string[] { "V1_value1", "V1_value2" };
        }
    }
}