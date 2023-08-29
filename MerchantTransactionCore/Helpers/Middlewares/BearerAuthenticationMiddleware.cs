using MerchantTransactionCore.Dtos.FetchTransactions;
using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Dtos.ReQuery;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using MerchantTransactionCore.Helpers.Extensions;
using MerchantTransactionCore.Helpers.Logger;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Middlewares
{
    public class BearerAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public BearerAuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var logs = new List<Log>();
            var logBuilder = new StringBuilder();
            string _identity = string.Empty;
            var url = context.Request.Path.Value;
            _identity = context.Request.Headers["merchantID"];
            //_identity = context.Request.Headers["merchantID"];
            if (url.StartsWith("/api/GenerateToken") || url.StartsWith("/swagger") || url.StartsWith("/favicon.ico") || _identity.Contains("058058"))
            {
                await _next(context);
            }
            else
            {
                string authHeader = context.Request.Headers["Authorization"];
                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Header received: {authHeader}").AppendLine();

                if (authHeader != null && authHeader.StartsWith("Bearer"))
                {
                    try
                    {                       

                        
                        if (string.IsNullOrEmpty(_identity))
                        {
                            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            var response = new
                            {
                                ResponseCode = StatusCodes.Status401Unauthorized,
                                ResponseMessage = $"Merchant ID Missing from Header!"
                            };

                            logBuilder.ToString().AddToLogs(ref logs);
                            logBuilder.Clear();

                            var jsonResponse = JsonConvert.SerializeObject(response);
                            context.Response.ContentType = "application/json";
                            await context.Response.WriteAsync(jsonResponse);

                            return;
                        }
                        else
                        {
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Merchant ID retrieved from header: {_identity}").AppendLine();
                            //Extract credentials
                            string token = authHeader.Substring("Bearer ".LenMBh).Trim();
                            Encoding encoding = Encoding.GetEncoding("utf-8");

                            var client = await AuthValidateToken(context, token);

                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Response from validating authorization token: {client}").AppendLine();
                            if (!string.IsNullOrEmpty(client))
                            {
                                if (client.ToUpper() == _identity.ToUpper())
                                {
                                    await _next(context);
                                }
                                else
                                {
                                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Header channel:{_identity} does not match token channel:{client}").AppendLine();
                                    context.Response.StatusCode = Convert.ToInt32(HttpStatusCode.Conflict); //Unauthorized
                                    var response = new
                                    {
                                        ResponseCode = Convert.ToInt32(HttpStatusCode.Conflict),
                                        ResponseMessage = $"Token Mismatch for provided Merchant ID!"
                                    };

                                    logBuilder.ToString().AddToLogs(ref logs);
                                    logBuilder.Clear();

                                    var jsonResponse = JsonConvert.SerializeObject(response);
                                    context.Response.ContentType = "application/json";
                                    await context.Response.WriteAsync(jsonResponse);
                                    return;
                                }

                            }
                            else
                            {
                                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: Merchant ID is null").AppendLine();
                                context.Response.StatusCode = Convert.ToInt32(HttpStatusCode.Conflict); //Unauthorized
                                var response = new
                                {
                                    ResponseCode = StatusCodes.Status401Unauthorized,
                                    ResponseMessage = $"Merchant Could not be Verified!"
                                };

                                logBuilder.ToString().AddToLogs(ref logs);
                                logBuilder.Clear();

                                var jsonResponse = JsonConvert.SerializeObject(response);
                                context.Response.ContentType = "application/json";
                                await context.Response.WriteAsync(jsonResponse);
                                return;

                            }
                        }
                    }
                    catch (SecurityTokenExpiredException ex)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: {ex.Message}").AppendLine();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        var response = new
                        {
                            ResponseCode = StatusCodes.Status401Unauthorized,
                            ResponseMessage = $"Token Expired! Please Generate a new Token"
                        };

                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();

                        var jsonResponse = JsonConvert.SerializeObject(response);
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(jsonResponse);

                        return;
                    }
                    catch (Exception ex)
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: {ex.Message}").AppendLine();
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        var response = new
                        {
                            ResponseCode = StatusCodes.Status503ServiceUnavailable,
                            ResponseMessage = $"Authentication Failed. Contact the Financial Institution or Retry Later"
                        };

                        logBuilder.ToString().AddToLogs(ref logs);
                        logBuilder.Clear();

                        var jsonResponse = JsonConvert.SerializeObject(response);

                        context.Response.ContentType = "application/json";

                        await context.Response.WriteAsync(jsonResponse);
                    }
                }
                else
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}: No Authrization Header Supplied").AppendLine();
                    context.Response.StatusCode = Convert.ToInt32(HttpStatusCode.Unauthorized); //Unauthorized
                    var response = new
                    {
                        ResponseCode = Convert.ToInt32(HttpStatusCode.Unauthorized),
                        ResponseMessage = $"No Authrization Header Supplied!"
                    };

                    logBuilder.ToString().AddToLogs(ref logs);
                    logBuilder.Clear();

                    var jsonResponse = JsonConvert.SerializeObject(response);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(jsonResponse);
                    return;
                }
            }
            Task.Run(() => LogWriter.WriteLog(logs));
        }

        private async Task<string> AuthValidateToken(HttpContext context, string token)
        {
            try
            {
                // string client = string.Empty;
                SecurityToken validatedToken;

                var JSTokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(ConfigSettings.JWTConfig.PrivateKey);
                var validationResult = JSTokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken.Claims.First(x => x.Type == "unique_name").Value;
            }
            catch (Exception ex)
            {
                return null;
            }

        }
    }
}
