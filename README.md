# Sp.Core.Webapi
.NET Core3.1通用后台接口模板框架

## 项目

[![API](https://img.shields.io/badge/接口API-Blog.Core-brightgreen.svg)](https://github.com/zjg1007/Sp.Core.Webapi/tree/master)

# 给个星星! ⭐️
如果你喜欢这个项目或者它帮助你, 请给 Star~（辛苦星咯）

*********************************************************
## 操作

### 安装.NET Core3.1SDK（往后会更新最新版本）
```
https://dotnet.microsoft.com/download
```


### 安装模板（存放项目的目录cmd命令执行）
```
dotnet new -i Sp.Core.Webapi
```

### 生成项目
```
dotnet new dotnetcoresp -n HelloBlog(项目名称)
```

### 启动项目
```
cd HelloBlog
dotnet restore(获取依赖)
dotnet build(构建项目)
cd HelloBlog.Core
dotnet run（运行调试）
```


## 两种调试方法
### 通过ToKen配置Bearer获取权限
1./api/Login/Token 获取ToKen令牌

![Logo](https://github.com/zjg1007/Sp.Core.Webapi/blob/master/Blog.Core/wwwroot/Bearer.png)

2.配置Bearer

![Logo](https://github.com/zjg1007/Sp.Core.Webapi/blob/master/Blog.Core/wwwroot/Token.png)

### 测试用户中间件

```
http://localhost:5001/noauth?userid=admin&&rolename=123  //请求地址，通过Url参数的形式，设置用户id和rolename
http://localhost:5001/noauth/reset   // 重置角色信息
```
   
   
   
系统环境

    windows 10、SQL server 2012、Visual Studio 2017、Windows Server 2008 R2

    后端技术：

      * .Net Core 3.1 API
      
      * Swagger 前后端文档说明，基于RESTful风格编写接口

      * Repository + Service 仓储模式编程

      * Async和Await 异步编程

      * Cors 简单的跨域解决方案

    数据库技术

      * SqlSugar 轻量级ORM框架

      * Autofac 轻量级IoC和DI依赖注入

      * AutoMapper 自动对象映射

    分布式缓存技术

      * Redis 轻量级分布式缓存
