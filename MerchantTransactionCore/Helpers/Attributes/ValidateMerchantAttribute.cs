using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Helpers.Common.ORM;
using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Dapper;

namespace MerchantTransactionCore.Helpers.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ValidateMerchantAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            string? value1 = string.Empty;
            try
            {
                if (string.IsNullOrWhiteSpace(value?.ToString()))
                {
                    return new ValidationResult($"The {validationContext.DisplayName} Field is Required");
                }

                value1 = value is object ? value.ToString() : "";

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
            catch (Exception ex)
            {
                return new ValidationResult("An Unknown Error Occurred, Please Try Again!");
            }

            try
            {
                //string customerID = validationContext.ObjectType.GetProperty("merchantID")?.GetValue(validationContext.ObjectInstance, null)?.ToString().Trim();
                string query = DatabaseStoredProcedures.Proc_ValidateMerchant;
                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {
                    DynamicParameters paras = new DynamicParameters();
                    paras.Add("@customer_id", string.IsNullOrWhiteSpace(value1) ? string.Empty : value1.Trim());

                    var storedProcedureReturnResponse = SqlMapper.Query<StoredProcedureReturnResponse<string>>(sqlConnection, query, paras, commandType: CommandType.StoredProcedure).FirstOrDefault();
                    
                    if (storedProcedureReturnResponse?._Message?.ToUpper() != "EXIST")
                    {
                        return new ValidationResult($"Merchant with Identity {value1} Not Fouund");
                    }
                }
            }
            catch(Exception ex)
            {
                return new ValidationResult("An Unknown Error Occurred, While Validating Merchant");
            }

            return ValidationResult.Success;
        }
    }
}
