using MerchantTransactionCore.Helpers.ActionFilters;
using Microsoft.OpenApi.Models;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using MerchantTransactionCore.Helpers.ConfigurationSettings;
using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.Extensions;
using MerchantTransactionCore.Helpers.Logger;
using AutoMapper;
using Microsoft.Extensions.Options;
using MerchantTransactionCore.Interfaces;
using MerchantTransactionCore.Repositories;
using MerchantTransactionCore.Services;
using Microsoft.Extensions.Caching.Memory;
using MerchantTransactionCore.Helpers.Middlewares;

namespace MerchantTransactionAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            ConfigurationSettingsHelper.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CORSPolicy", builder =>
                {
                    builder.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
                });
            });

            services.AddControllers(opt =>
            {
                opt.Filters.Add(typeof(ValidationActionFilter));
                opt.ReturnHttpNotAcceptable = false;
            })
            .AddNewtonsoftJson(opt =>
            {
                opt.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Error;
            })
            .ConfigureApiBehaviorOptions(opt =>
            {
                opt.SuppressModelStateInvalidFilter = true;
            });

            IHttpContextAccessor httpContextAccessor = new HttpContextAccessor();
            MyHttpContextAccessor.HttpContextAccessor = httpContextAccessor;

            //services.AddSingleton<IHttpContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddEndpointsApiExplorer();
            services.AddServiceLibraryServices();

            services.AddScoped<IAccessControl, AccessControl>();          
            services.AddScoped<IMerchant, Merchant>();          
            //services.AddScoped<IMemoryCache, CachingMiddleware>();          
            

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MBCollections Transactions Re-Query", Version = "v1", Description = "API to requery MBCollection transactions for merchants" });


                // Bearer authentication scheme for JWT tokens
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { 
                    Description = "Authorization header using the Bearer Scheme (JWT token). \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...\"", 
                    Name = "Authorization", 
                    In = ParameterLocation.Header, 
                    Type = SecuritySchemeType.ApiKey, 
                    Scheme = "Bearer"
                });

                c.AddSecurityDefinition("CustomHeader", new OpenApiSecurityScheme {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Header,
                    Name = "merchantID",
                    Description = "Enter Merchant ID here"
                });

                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
            });
        }

        // Configure HTTP request Pipeline on runtime
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IHttpContextAccessor httpContextAccessor)
        {
            MyHttpContextAccessor.HttpContextAccessor = httpContextAccessor;
            LogWriter.Logger = logger;
            app.AddCustomMiddlewares();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //app.UseSwagger();
                //app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MBCollectionGENERICAPI v1"));
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CORSPolicy");

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("./v1/swagger.json", "MBCollectionGeneral_Api v1"));

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
