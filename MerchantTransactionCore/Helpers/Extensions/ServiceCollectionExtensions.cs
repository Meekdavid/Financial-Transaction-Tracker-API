using MerchantTransactionCore.Helpers.MapperProfiles;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;

namespace MerchantTransactionCore.Helpers.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceLibraryServices(this IServiceCollection services)
        {            
            services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

            return services;
        }
    }
}
