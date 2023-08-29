using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RecyclableMemoryStreamManager _recyclableMemoryStreamManager;

        public RequestResponseLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
            _recyclableMemoryStreamManager = new RecyclableMemoryStreamManager();
        }

        public async Task Invoke(HttpContext context)
        {
            await LogRequest(context);
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            context.Request.EnableBuffering();

            using (var requestStream = _recyclableMemoryStreamManager.GetStream())
            {
                await context.Request.Body.CopyToAsync(requestStream);
                var log = $"Http Request Information:{Environment.NewLine}Schema:{context.Request.Scheme}{Environment.NewLine}Host: {context.Request.Host}{Environment.NewLine}Path: {context.Request.Path}{Environment.NewLine}Method: {context.Request.Method}{Environment.NewLine}QueryString: {context.Request.QueryString}{Environment.NewLine}Request Body: {ReadStreamInChunks(requestStream)}";
                LogWriter.WriteLog(log, Utils.LoMBype.LOG_INFORMATION);
            }

            context.Request.Body.Position = 0;
        }

        private static string ReadStreamInChunks(Stream stream)
        {
            const int readChunkBufferLenMBh = 4096;

            stream.Seek(0, SeekOrigin.Begin);
            using (var textWriter = new StringWriter())
            {
                using (var reader = new StreamReader(stream))
                {
                    var readChunk = new char[readChunkBufferLenMBh];
                    int readChunkLenMBh;

                    do
                    {
                        readChunkLenMBh = reader.ReadBlock(readChunk,
                                                           0,
                                                           readChunkBufferLenMBh);
                        textWriter.Write(readChunk, 0, readChunkLenMBh);
                    } while (readChunkLenMBh > 0);

                    return textWriter.ToString();
                }
            }
        }

        private async Task LogResponse(HttpContext context)
        {
            var originalBodyStream = context.Response.Body;

            using (var responseBody = _recyclableMemoryStreamManager.GetStream())
            {
                context.Response.Body = responseBody;

                await _next(context);

                context.Response.Body.Seek(0, SeekOrigin.Begin);
                //var text = await new StreamReader(context.Response.Body).ReadToEndAsync();
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var log = $"Http Response Information:{Environment.NewLine}Schema:{context.Request.Scheme}{Environment.NewLine}Host: {context.Request.Host}{Environment.NewLine}Path: {context.Request.Path}{Environment.NewLine}Method: {context.Request.Method}{Environment.NewLine}QueryString: {context.Request.QueryString}";
                LogWriter.WriteLog(log, Utils.LoMBype.LOG_INFORMATION);

                await responseBody.CopyToAsync(originalBodyStream);
            }
        }
    }
}
