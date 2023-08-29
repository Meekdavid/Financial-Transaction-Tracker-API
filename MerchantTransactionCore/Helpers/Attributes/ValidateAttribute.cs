using MerchantTransactionCore.Helpers.Common;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ValidateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {

            try
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    return new ValidationResult($"The {validationContext.DisplayName} Field is Required");
                }

                string? value1 = value is object ? value.ToString() : "";
                var otherProperty = MyHttpContextAccessor.GetHttpContextAccessor().HttpContext.Request.Headers.TryGetValue("merchantID", out var stringVal);
                string _Channel = stringVal[0].ToString();

                if (!string.IsNullOrEmpty(value1))
                {
                    //XML Injection Checks
                    string pattern = @"[<>&'$=]|(\bOR\b)";
                    if (Regex.IsMatch(value1, pattern, RegexOptions.IgnoreCase))
                    {
                        return new ValidationResult($"The {validationContext.DisplayName} Contains Special Character");
                    }

                    value1 = Regex.Replace(value1, "<.*?>", string.Empty);
                }

                validationContext.ObjectType.GetProperty(validationContext.MemberName).SetValue(validationContext.ObjectInstance, value1, null);
            }
            catch(Exception ex)
            {
                return new ValidationResult("An Unknown Error Occurred, Please Try Again!");
            }
            return ValidationResult.Success;
        }
    }
}
