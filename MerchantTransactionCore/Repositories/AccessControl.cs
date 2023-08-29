using AutoMapper;
using MerchantTransactionCore.Dtos.GenerateToken;
using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Dtos.Models;
using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.Common.ORM;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using MerchantTransactionCore.Helpers.Extensions;
using MerchantTransactionCore.Helpers.Logger;
using MerchantTransactionCore.Interfaces;
using MerchantTransactionCore.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MerchantTransactionCore.Helpers.Common.Utils;

namespace MerchantTransactionCore.Repositories
{
    public class AccessControl : IAccessControl
    {
        private string className = string.Empty;
        private readonly IMapper _mapper;

        public AccessControl(IMapper mapper)
        {
            _mapper = mapper;
            className = GetType().Name;
        }

        public async Task<MethodReturnResponse<MerchantValidationResponse>> AuthenticateMerchant(TokenRequest tokenRequest, string merchantID)
        {
            string methodName = "AuthenticateMerchant", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Authenticate Merchant Credentials").AppendLine();

            try
            {
                string query = DatabaseStoredProcedures.Proc_AuthenticateMerchant;

                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {
                    var paras = new Dictionary<string, SP_ParameterWrapper>();
                    paras.Add("@customer_id", new SP_ParameterWrapper(string.IsNullOrWhiteSpace(merchantID) ? string.Empty : merchantID.Trim()));
                    paras.Add("@AuthKey", new SP_ParameterWrapper(string.IsNullOrWhiteSpace(tokenRequest.secretKey) ? string.Empty : tokenRequest.secretKey.Trim()));

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About Authenticating Merchant Credentials from the DB with MerchantID {merchantID}.").AppendLine();
                    var storedProcedureReturnResponse = await DapperUtility<StoredProcedureReturnResponse<string>>.GetObjectAsync(sqlConnection, paras, query, CommandType.StoredProcedure);
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Back from Authenticating Merchant with Response: {JsonConvert.SerializeObject(storedProcedureReturnResponse)}").AppendLine();

                    var AuthenticationResponseToReturn = _mapper.Map<DataBaseAuthenticationResponse>(storedProcedureReturnResponse);

                    if (!((storedProcedureReturnResponse?._Success).HasValue && storedProcedureReturnResponse._Success))
                    {
                        //ERROR DURING DB OPERATION
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered Authenticating Merchant from the DB").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new MethodReturnResponse<MerchantValidationResponse>()
                        {                            
                            Logs = logs,
                            objectValue = new MerchantValidationResponse()
                            {
                                ResponseCode = Utils.StatusCode_StoredProcedureError,
                                ResponseDescription = $"{Utils.StatusMessage_StoredProcedureError} | {storedProcedureReturnResponse._Message}"
                            }
                        };
                    }
                    else
                    {
                        string authenticationMessage = AuthenticationResponseToReturn._Message;

                        if (authenticationMessage == null)
                        {
                            //ERROR DURING DB OPERATION
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered While Authenticating Merchant").AppendLine();
                            logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                            logBuilder.ToString().AddToLogs(ref logs);

                            return new MethodReturnResponse<MerchantValidationResponse>()
                            {                                
                                Logs = logs,
                                objectValue = new MerchantValidationResponse()
                                {
                                    ResponseCode = Utils.StatusCode_Success,
                                    ResponseDescription = Utils.StatusMessage_AccountNameNotFound
                                }
                            };
                        }

                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Authenticating Merchant from DB was Successful, and Merchant is {authenticationMessage}").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        logBuilder.ToString().AddToLogs(ref logs);

                        return new MethodReturnResponse<MerchantValidationResponse>()
                        {                           
                            Logs = logs,
                            objectValue = new MerchantValidationResponse()
                            {
                                ResponseCode = Utils.StatusCode_Success,
                                ResponseDescription = authenticationMessage
                            }
                        };
                    }
                }

            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Authenticate Merchant Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Authenticating Merchant").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                if (ex.GetType().Equals(typeof(SqlException)))
                {
                    return new MethodReturnResponse<MerchantValidationResponse>()
                    {                        
                        Logs = logs,
                        objectValue = new MerchantValidationResponse()
                        {
                            ResponseCode = Utils.StatusCode_DatabaseConnectionError,
                            ResponseDescription = Utils.StatusMessage_DatabaseConnectionError
                        }
                    };                    
                }

                return new MethodReturnResponse<MerchantValidationResponse>
                {
                    Logs = logs,
                    objectValue = new MerchantValidationResponse()
                    {
                        ResponseCode = Utils.StatusCode_ExceptionError,
                        ResponseDescription = Utils.StatusMessage_AuthenticationFailue
                    }
                };
            }
            
        }

        public async Task<MethodReturnResponse<GetTokenResponse>> GetAccessToken(TokenRequest tokenRequest, string merchantID)
        {
            string methodName = "GetAccessToken", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Generate Token for Merchant {merchantID}").AppendLine();

            try
            {
                var authenticationResponse = await AuthenticateMerchant(tokenRequest, merchantID);
                //RECONCILE THE LOGS
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                authenticationResponse.Logs.AddToLogs(ref logs);

                if (authenticationResponse.objectValue.ResponseDescription.ToUpper() == "AUTHENTICATED")
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} About to Generate Token for Merchant {merchantID}").AppendLine();

                    var JSTokenHandler = new JwtSecurityTokenHandler();
                    var tokenResponse = AccessControlService.GenerateJwtToken(merchantID);

                    if (!string.IsNullOrEmpty(tokenResponse.ToString()))
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Token Sucessfully Generated").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        var tokenReturnResponse = new TokenDetails
                        {
                            Token = JSTokenHandler.WriteToken(tokenResponse),
                            ExpiresIn = $"{(tokenResponse.ValidTo - tokenResponse.ValidFrom).TotalMinutes} minutes",
                            ExpiryDate = tokenResponse.ValidTo.Date,
                            TokenType = "Bearer"
                        };

                        return new MethodReturnResponse<GetTokenResponse>
                        {
                            Logs = logs,
                            objectValue = new GetTokenResponse()
                            {
                                AccessToken = tokenReturnResponse,
                                ResponseCode = Utils.StatusCode_Success,
                                ResponseDescription = Utils.StatusMessage_Success
                            }
                        };
                    }
                    else
                    {
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Generated Token was null").AppendLine();
                        logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                        return new MethodReturnResponse<GetTokenResponse>
                        {
                            Logs = logs,
                            objectValue = new GetTokenResponse()
                            {
                                AccessToken = null,
                                ResponseCode = Utils.StatusCode_TokenNullValue,
                                ResponseDescription = Utils.StatusMessage_TokenNullValue
                            }
                        };
                    }
                }
                else
                {
                    logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                    return new MethodReturnResponse<GetTokenResponse>
                    {
                        Logs = logs,
                        objectValue = new GetTokenResponse()
                        {
                            AccessToken = null,
                            ResponseCode = authenticationResponse.objectValue.ResponseCode,
                            ResponseDescription = authenticationResponse.objectValue.ResponseDescription
                        }
                    };
                }
                

            }
            catch( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Generate Token Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Generating Token for Merchant").AppendLine();
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                return new MethodReturnResponse<GetTokenResponse>
                {
                    Logs = logs,
                    objectValue = new GetTokenResponse()
                    {
                        AccessToken = null,
                        ResponseCode = Utils.StatusCode_ExceptionError,
                        ResponseDescription = Utils.StatusMessage_UnknownError
                    }
                };
            }
            finally
            {
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }
        }
    }
}
