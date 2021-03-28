using Blog.Core.Model.See;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Blog.Core.Extensions.ServiceExtensions
{
   public static class DbSetup
    {
        public static void AddDbSetup(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddScoped<DBSeed>();
            services.AddScoped<MyContext>();
        }
    }
}
