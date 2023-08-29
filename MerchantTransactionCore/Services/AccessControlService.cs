using MerchantTransactionCore.Dtos.Models;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Services
{
    public static class AccessControlService
    {
        public static SecurityToken GenerateJwtToken(string merchantID)
        {            
            var JSTokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var key = Encoding.UTF8.GetBytes(ConfigSettings.JWTConfig.PrivateKey);
                var secTokenDescriptor = new SecurityTokenDescriptor
                {
                    TokenType = "Bearer",
                    Expires = DateTime.UtcNow.AddMinutes(ConfigSettings.JWTConfig.TokenExpiration),
                    Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, merchantID)
                    }),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                return JSTokenHandler.CreateToken(secTokenDescriptor);
            }
            catch (Exception ex)
            {
                throw;
            }            
        }
    }
}
