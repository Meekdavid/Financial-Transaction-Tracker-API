using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Middlewares
{
    public class CachingMiddleware
    {
        private readonly RequestDelegate _next;
        //private readonly IMemoryCache _cache;
        private static readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

        public CachingMiddleware(RequestDelegate next /*IMemoryCache cache*/)
        {
            _next = next;
            //_cache = cache;
        }

        public async Task Invoke(HttpContext context)
        {
            var url = context.Request.Path.Value;
            if (url.StartsWith("/api/GenerateToken") || url.StartsWith("/api/ChangeSecretKey") || url.StartsWith("/swagger") || url.StartsWith("/favicon.ico") || url == "/swagger/index.html")
            {
                //await _next(context);
                await _next.Invoke(context);
            }            

            // Generate a cache key based on the complete request
            var cacheKey = GenerateCacheKey(context);

            if (_cache.TryGetValue(cacheKey, out var cachedResponse))
            {
                // Set the content type to indicate JSON response
                context.Response.ContentType = "application/json";
                // If cached, return the cached response
                await context.Response.WriteAsync(cachedResponse.ToString());
                return;
            }

            // If not cached, proceed to the next middleware in the pipeline
            var originalBodyStream = context.Response.Body;

            using (var memoryStream = new System.IO.MemoryStream())
            {
                // Intercept the response and write it to the memory stream
                context.Response.Body = memoryStream;

                await _next(context);

                // Get the response content from the memory stream
                var responseContent = Newtonsoft.Json.JsonConvert.SerializeObject(await FormatResponse(memoryStream));

                // Store the response in the cache for a certain duration (e.g., 5 minutes)
                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ConfigSettings.ApplicationSetting.ResponseCacheTime)
                };
                _cache.Set(cacheKey, responseContent, cacheEntryOptions);

                // Write the response content to the original response stream
                memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
                await memoryStream.CopyToAsync(originalBodyStream);
            }
        }

        private async Task<string> GenerateCacheKey(HttpContext context)
        {
            // Generate a unique cache key based on the complete request
            var keyBuilder = new StringBuilder();
            keyBuilder.Append(context.Request.Path);

            foreach (var header in context.Request.Headers)
            {
                //keyBuilder.Append(header.Key);
                keyBuilder.Append(header.Value);
            }

            if (context.Request.Body != null && context.Request.Method == "POST")
            {
                context.Request.Body.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(context.Request.Body);
                var requestBody = await reader.ReadToEndAsync();

                // Parse the JSON request body to extract the unique parameters
                var jsonBody = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(requestBody);

                var url = context.Request.Path.Value;
                if (url.StartsWith("/api/TransactionRequery"))
                {
                    if (jsonBody.TryGetValue("transactionReference", out var reference))
                    {
                        keyBuilder.Append(reference);
                    }
                }

                if (url.StartsWith("/api/TransactionsHistory"))
                {
                    if (jsonBody.TryGetValue("startDate", out var reference))
                    {
                        keyBuilder.Append(reference);
                    }
                    if (jsonBody.TryGetValue("endDate", out var reference1))
                    {
                        keyBuilder.Append(reference1);
                    }
                }
            }

            return keyBuilder.ToString();
        }

        private async Task<string> FormatResponse(System.IO.MemoryStream memoryStream)
        {
            // Read the memory stream and convert
            memoryStream.Seek(0, System.IO.SeekOrigin.Begin);
            using var reader = new System.IO.StreamReader(memoryStream);
            var content = await reader.ReadToEndAsync();
            return content;
        }
    }

}
