using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Services
{
    public static class Caching
    {
        private static readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public static void AddCache(string channel, string key)
        {
            try
            {
                var cacheExpiryOptions = new MemoryCacheEntryOptions();
                cacheExpiryOptions.AbsoluteExpiration = DateTime.Now.AddSeconds(ConfigSettings.ApplicationSetting.OtherServicesCacheTime);
                cacheExpiryOptions.Priority = CacheItemPriority.High;
                cacheExpiryOptions.SlidingExpiration = TimeSpan.FromSeconds(ConfigSettings.ApplicationSetting.OtherServicesCacheTime - 2400);

                _memoryCache.Set(channel, key, cacheExpiryOptions);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static string getKeyFromCache(string channel)
        {
            try
            {
                var result = _memoryCache.Get(channel);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
