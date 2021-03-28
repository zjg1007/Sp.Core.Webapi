using Blog.Core.Common;
using InitQ.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisMqController : Controller
    {
        readonly IRedisBasketRepository _redisBasketRepository;

        public RedisMqController(IRedisBasketRepository redisBasketRepository)
        {
            _redisBasketRepository = redisBasketRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task< object> Get()
        {
            var msg = "这里是一条日志";
            await _redisBasketRepository.ListLeftPushAsync(RedisMqKey.Loging, msg);
            return Ok();
        }
    }
}
