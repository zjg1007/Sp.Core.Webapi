using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blog.Core.Common;
using Blog.Core.IRepository.UnitOfWork;
using Blog.Core.IServices;
using Blog.Core.Model;
using Blog.Core.Model.Models;
using Blog.Core.SwaggerHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Profiling;

namespace Blog.Core.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize("Permission")]
   // [Authorize(Policy ="Admin")]
    public class BlogController : ControllerBase
    {
        readonly IBlogArticleServices _blogArticleServices;
        readonly IUnitOfWork _unitOfWork;

        public BlogController(IUnitOfWork unitOfWork,IBlogArticleServices blogArticleServices )
        {
            _blogArticleServices = blogArticleServices;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 测试仓储层到领域层到服务层的DTO
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [HttpGet]
        //[Authorize(Policy = "SystemOrAdmin")]
        //[ApiExplorerSettings(GroupName = "V1")]
        //[CustomRoute(ApiVersions.V2, "Get")]
        public async Task<PageModel<BlogArticle>> Get( int id, int page = 1)
        {
            //每页大小
            var pageSize = 10;
            //var connect = Appsettings.app(new string[] { "AppSettings", "RedisCaching", "ConnectionString" });//按照层级的顺序，依次写出来
            List<BlogArticle> blogArticleList = new List<BlogArticle>();
            blogArticleList = await _blogArticleServices.GetBlogs();
            // 你可以用这种包括的形式
            using (MiniProfiler.Current.Step("开始加载数据："))
            {
                //先查找Redis缓存中有没有这个数据,有就直接带出来，不需要做DB操作，没有的话更新缓存,缓存时间为2小时
                //if (_redisCacheManager.Get<List<BlogArticle>>("Redis.Blog").Result != null)
                //{
                //    // 也可以直接这么写
                //    MiniProfiler.Current.Step("从Redis服务器中加载数据：");
                //    blogArticleList =await _redisCacheManager.Get<List<BlogArticle>>("Redis.Blog");
                //}
                //else
                //{
                //    // 也可以直接这么写
                //    MiniProfiler.Current.Step("从MSSQL服务器中加载数据：");
                //    blogArticleList = await _blogArticleServices.GetBlogs();
                //  await  _redisCacheManager.Set("Redis.Blog", blogArticleList, TimeSpan.FromHours(2));//缓存2小时
                //}
                //MiniProfiler.Current.Step("处理成功之后,开始处理最终数据：");
                var pageModel = new PageModel<BlogArticle>
                {
                    page = page,
                    pageCount = (blogArticleList.Count + pageSize - 1) / pageSize,
                    dataCount = blogArticleList.Count,
                    PageSize = pageSize,
                    data = blogArticleList.Skip((page - 1) * pageSize).Take(pageSize).ToList(),
                    success = true
                };
                //blogArticleList = await _blogArticleServices.GetBlogs();
                //_redisCacheManager.Set("Redis.Blog", blogArticleList, TimeSpan.FromHours(2));//缓存2小时
                return pageModel;
            }
           
        }
        /// <summary>
        /// 获取博客详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        //[Authorize(Permissions.Name)]
        [Authorize]
        //[Authorize(Roles = "Admin")]
        public async Task<object> Get(int id)
        {
            var model = await _blogArticleServices.GetBlogDetails(id);
            return Ok(new
            {
                success = true,
                data = model
            });
        }


      
    }
}