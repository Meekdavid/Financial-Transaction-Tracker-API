using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using MerchantTransactionCore.Helpers.Middlewares;
using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder AddCustomMiddlewares(this IApplicationBuilder app)
        {
            app.UseMiddleware<RequestResponseLoggingMiddleware>();
            app.UseMiddleware<BearerAuthenticationMiddleware>();
            //if (ConfigSettings.ApplicationSetting.ActivateResponseCaching)
            //{
            //    app.UseMiddleware<CachingMiddleware>();
            //}
            return app;
        }
    }
}
