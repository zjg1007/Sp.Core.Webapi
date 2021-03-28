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
     public class RoleModulePermissionController : ControllerBase
        {
             /// <summary>
             /// 服务器接口，因为是模板生成，所以首字母是大写的，自己可以重构下
             /// </summary>
            private readonly IRoleModulePermissionServices _roleModulePermissionServices;
    
            public RoleModulePermissionController(IRoleModulePermissionServices RoleModulePermissionServices)
            {
                _roleModulePermissionServices = RoleModulePermissionServices;
            }
    
            [HttpGet]
            public async Task<MessageModel<PageModel<RoleModulePermission>>> Get(int page = 1, string key = "",int intPageSize = 50)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(key))
                {
                    key = "";
                }
    
                Expression<Func<RoleModulePermission, bool>> whereExpression = a => a.Id > 0;
    
                return new MessageModel<PageModel<RoleModulePermission>>()
                {
                    msg = "获取成功",
                    success = true,
                    response = await _roleModulePermissionServices.QueryPage(whereExpression, page, intPageSize)
                };

    }

    [HttpGet("{id}")]
    public async Task<MessageModel<RoleModulePermission>> Get(int id = 0)
    {
        return new MessageModel<RoleModulePermission>()
        {
            msg = "获取成功",
            success = true,
            response = await _roleModulePermissionServices.QueryById(id)
        };
    }

    [HttpPost]
    public async Task<MessageModel<string>> Post([FromBody] RoleModulePermission request)
    {
        var data = new MessageModel<string>();

        var id = await _roleModulePermissionServices.Add(request);
        data.success = id > 0;

        if (data.success)
        {
            data.response = id.ObjToString();
            data.msg = "添加成功";
        }

        return data;
    }

    [HttpPut]
    public async Task<MessageModel<string>> Put([FromBody] RoleModulePermission request)
    {
        var data = new MessageModel<string>();
        if (request.Id > 0)
        {
            data.success = await _roleModulePermissionServices.Update(request);
            if (data.success)
            {
                data.msg = "更新成功";
                data.response = request?.Id.ObjToString();
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
            var detail = await _roleModulePermissionServices.QueryById(id);

            detail.IsDeleted = true;

                if (detail != null)
                {
                    data.success = await _roleModulePermissionServices.Update(detail);
                    if (data.success)
                    {
                        data.msg = "删除成功";
                        data.response = detail?.Id.ObjToString();
                    }
                }
        }

        return data;
    }
}
}