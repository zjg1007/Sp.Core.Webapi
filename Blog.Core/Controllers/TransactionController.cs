using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.Common;
using Blog.Core.IRepository.UnitOfWork;
using Blog.Core.IServices;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        readonly IPasswordLibServices _passwordLibServices;
        readonly IRedisBasketRepository _redisCacheManager;
        readonly IUnitOfWork _unitOfWork;
        readonly IGuestbookServices _guestbookServices;


        public TransactionController(IGuestbookServices guestbookServices, IUnitOfWork unitOfWork, IPasswordLibServices passwordLibServices, IRedisBasketRepository redisCacheManager)
        {
            _passwordLibServices = passwordLibServices;
            _redisCacheManager = redisCacheManager;
            _unitOfWork = unitOfWork;
            _guestbookServices = guestbookServices;
        }

        [HttpGet]
        public async Task<IEnumerable<string>> Get()
        {
            try
            {
                Console.WriteLine($"");
                //开始事务
                Console.WriteLine($"Begin Transaction");
                _unitOfWork.BeginTran();
                Console.WriteLine($"");
                var passwords = await _passwordLibServices.Query();
                // 第一次密码表的数据条数
                Console.WriteLine($"first time : the count of passwords is :{passwords.Count}");

                // 向密码表添加一条数据
                Console.WriteLine($"insert a data into the table PasswordLib now.");
                var insertPassword = await _passwordLibServices.Add(new PasswordLib()
                {
                    IsDeleted = false,
                    plAccountName = "aaa",
                    plCreateTime = DateTime.Now
                });

                // 第二次查看密码表有多少条数据，判断是否添加成功
                passwords = await _passwordLibServices.Query(d => d.IsDeleted == false);
                Console.WriteLine($"second time : the count of passwords is :{passwords.Count}");

                //......

                Console.WriteLine($"");
                var guestbooks = await _guestbookServices.Query();
                Console.WriteLine($"first time : the count of guestbooks is :{guestbooks.Count}");

                int ex = 0;
                // 出现了一个异常！
                Console.WriteLine($"\nThere's an exception!!");
                int throwEx = 1 / ex;

                Console.WriteLine($"insert a data into the table Guestbook now.");
                var insertGuestbook = await _guestbookServices.Add(new Guestbook()
                {
                    username = "bbb",
                    blogId = 1,
                    createdate = DateTime.Now,
                    isshow = true
                });

                guestbooks = await _guestbookServices.Query();
                Console.WriteLine($"second time : the count of guestbooks is :{guestbooks.Count}");

                //事务提交
                _unitOfWork.CommitTran();
            }
            catch (Exception)
            {
                // 事务回滚
                _unitOfWork.RollbackTran();
                var passwords = await _passwordLibServices.Query();
                // 第三次查看密码表有几条数据，判断是否回滚成功
                Console.WriteLine($"third time : the count of passwords is :{passwords.Count}");

                var guestbooks = await _guestbookServices.Query();
                Console.WriteLine($"third time : the count of guestbooks is :{guestbooks.Count}");
            }

            return new string[] { "value1", "value2" };
        }
        /// <summary>
        /// 测试AOP事务
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TestTranInRepositoryAOP")]
        public async Task<string> TestTranInRepositoryAOP(int id)
        {
            await  _guestbookServices.TestTranInRepositoryAOP();
            return "";
        }

    }
}