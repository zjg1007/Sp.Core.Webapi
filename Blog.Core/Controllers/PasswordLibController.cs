using Blog.Core.IServices;
using Blog.Core.Model;
using Blog.Core.Model.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Blog.Core.Controllers
{
	[Route("api/[controller]/[action]")]
	[ApiController]
    [Authorize(Permissions.Name)]
     public class PasswordLibController : ControllerBase
        {
             /// <summary>
             /// 服务器接口，因为是模板生成，所以首字母是大写的，自己可以重构下
             /// </summary>
            private readonly IPasswordLibServices _passwordLibServices;
    
            public PasswordLibController(IPasswordLibServices PasswordLibServices)
            {
                _passwordLibServices = PasswordLibServices;
            }
    
            [HttpGet]
            public async Task<MessageModel<PageModel<PasswordLib>>> Get(int page = 1, string key = "",int intPageSize = 50)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
                {
                    key = "";
                }
    
                Expression<Func<PasswordLib, bool>> whereExpression = a => a.PLID > 0;
    
                return new MessageModel<PageModel<PasswordLib>>()
                {
                    msg = "获取成功",
                    success = true,
                    response = await _passwordLibServices.QueryPage(whereExpression, page, intPageSize)
                };

    }

    [HttpGet("{id}")]
    public async Task<MessageModel<PasswordLib>> Get(int id = 0)
    {
        return new MessageModel<PasswordLib>()
        {
            msg = "获取成功",
            success = true,
            response = await _passwordLibServices.QueryById(id)
        };
    }

    [HttpPost]
    public async Task<MessageModel<string>> Post([FromBody] PasswordLib request)
    {
        var data = new MessageModel<string>();

        var id = await _passwordLibServices.Add(request);
        data.success = id > 0;

        if (data.success)
        {
            data.response = id.ObjToString();
            data.msg = "添加成功";
        }

        return data;
    }

    [HttpPut]
    public async Task<MessageModel<string>> Put([FromBody] PasswordLib request)
    {
        var data = new MessageModel<string>();
        if (request.PLID > 0)
        {
            data.success = await _passwordLibServices.Update(request);
            if (data.success)
            {
                data.msg = "更新成功";
                data.response = request?.PLID.ObjToString();
            }
        }

        return data;
    }

    [HttpDelete("{id}")]
    public async Task<MessageModel<string>> Delete(int id = 0)
    {
        var data = new MessageModel<string>();
        if (id > 0)
        {
            var detail = await _passwordLibServices.QueryById(id);

            detail.IsDeleted = true;

                if (detail != null)
                {
                    data.success = await _passwordLibServices.Update(detail);
                    if (data.success)
                    {
                        data.msg = "删除成功";
                        data.response = detail?.PLID.ObjToString();
                    }
                }
        }

        return data;
    }
}
}