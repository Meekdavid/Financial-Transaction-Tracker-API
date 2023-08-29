using MerchantTransactionCore.Dtos.ChangeSecret;
using MerchantTransactionCore.Dtos.FetchTransactions;
using MerchantTransactionCore.Dtos.GenerateToken;
using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Dtos.Models;
using MerchantTransactionCore.Dtos.ReQuery;
using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using MerchantTransactionCore.Helpers.Extensions;
using MerchantTransactionCore.Helpers.Logger;
using MerchantTransactionCore.Interfaces;
using MerchantTransactionCore.Repositories;
using MerchantTransactionCore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Reflection;
using System.Text;
using static MerchantTransactionCore.Helpers.Common.Utils;

namespace MerchantTransactionAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class MBCollectionServiceController : Controller
    {
        private readonly string className = string.Empty;
        private readonly IAccessControl _accessControl;
        private readonly IMerchant _merchant;
        public bool theMaster;
        public MBCollectionServiceController(IAccessControl accessControl, IMerchant merchant)
        {
            className = GetType().Name;
            _accessControl = accessControl;
            _merchant = merchant;
        }

        [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPost("GenerateToken")]
        public async Task<ActionResult<TokenResponse>> GenerateToken([FromBody] TokenRequest tokenRequest, [FromHeader] string merchantID)
        {
            string classAndMethodName = $"GenerateToken|{className}";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for Getting Merchant Token. Payload: {JsonConvert.SerializeObject(tokenRequest)}").AppendLine();
            var returnHttpStatusCode = Utils.HttpStatusCode_Ok;
            var generateTokenResponse = new TokenResponse();

            var IPEntity = SourceIPLogger.logCallingIP();
            logBuilder.AppendLine($" The Endpoint 'GenerateToken' was accessed by IPs : {IPEntity.IPAddress} with host: " +
                $"{IPEntity.userAgent} on: {DateTime.Now}, with the requestID {tokenRequest.requestID}").AppendLine();

            try
            {
                var potentialDuplicateID = Caching.getKeyFromCache(merchantID);
                if (potentialDuplicateID == tokenRequest.requestID)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Duplicate RequestID Identified").AppendLine();
                    generateTokenResponse.ResponseCode = Utils.StatusCode_Failure;
                    generateTokenResponse.ResponseMessage = Utils.StatusMessage_Duplicate;
                    return StatusCode(returnHttpStatusCode, generateTokenResponse);
                }

                var validateMerchant = await _merchant.IsAllowedMerchant(merchantID);
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                validateMerchant.Logs.AddToLogs(ref logs);

                if (!validateMerchant.objectValue.IsValidInput)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID recieved from the Header contains disallowed special characters").AppendLine();
                    generateTokenResponse.ResponseCode = Utils.StatusCode_BadRequest;
                    generateTokenResponse.ResponseMessage = Utils.StatusMessage_InputFailure;
                    return StatusCode(returnHttpStatusCode, generateTokenResponse);
                }

                if (!validateMerchant.objectValue.IsValidMerchant)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID supplied is not found on 'Customers' table on MBCollection DB.").AppendLine();
                    generateTokenResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                    generateTokenResponse.ResponseMessage = Utils.StatusMessage_AccountNameNotFound;
                    return StatusCode(returnHttpStatusCode, generateTokenResponse);
                }

                var generatedToken = await _accessControl.GetAccessToken(tokenRequest, merchantID);
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                generatedToken.Logs.AddToLogs(ref logs);

                if (generatedToken.objectValue.AccessToken != null)
                {
                    var merchantDetails = await _merchant.GetMerchantDetails(merchantID);

                    generateTokenResponse.ResponseCode = generatedToken.objectValue.ResponseCode;
                    generateTokenResponse.ResponseMessage = generatedToken.objectValue.ResponseDescription;
                    generateTokenResponse.TokenDetails = generatedToken.objectValue.AccessToken;
                    generateTokenResponse.MerchantDetails = merchantDetails.objectValue;
                    Caching.AddCache(merchantID, tokenRequest.requestID);
                }
                else
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Token was Unable to be Generated, Value returned Null.").AppendLine();
                    
                    generateTokenResponse.ResponseCode = Utils.StatusCode_Failure;
                    generateTokenResponse.ResponseMessage = generatedToken.objectValue.ResponseDescription;
                }
            }
            catch ( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Generate Token Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered on Token Generation Endpoint while Generating Token for Merchant").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                generateTokenResponse.ResponseCode = Utils.StatusCode_ExceptionError;
                generateTokenResponse.ResponseMessage = Utils.StatusMessage_ExceptionError;
                return StatusCode(returnHttpStatusCode, generateTokenResponse);
            }
            finally
            {
                Caching.AddCache(merchantID, tokenRequest.requestID);
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            Task.Run(() => LogWriter.WriteLog(logs));
            return StatusCode(returnHttpStatusCode, generateTokenResponse);
        }

        [ProducesResponseType(typeof(ReQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPost("TransactionRequery")]
        public async Task<ActionResult<ReQueryResponse>> TransactionRequery([FromBody] ReQueryRequest reQueryRequest, [FromHeader] string merchantID)
        {
            string classAndMethodName = $"TransactionRequery|{className}";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for Transaction Requery. Payload: {JsonConvert.SerializeObject(reQueryRequest)}").AppendLine();
            var returnHttpStatusCode = Utils.HttpStatusCode_Ok;
            var requeryResponse = new ReQueryResponse();

            var IPEntity = SourceIPLogger.logCallingIP();
            logBuilder.AppendLine($" The Endpoint 'TransactionRequery' was accessed by IPs : {IPEntity.IPAddress} with host: " +
                $"{IPEntity.userAgent} on: {DateTime.Now}, with the requestID {reQueryRequest.requestID}").AppendLine();

            try
            {
                var potentialDuplicateID = Caching.getKeyFromCache(merchantID);
                if (potentialDuplicateID == reQueryRequest.requestID)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Duplicate RequestID Identified").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Failure;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_Duplicate;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                var validateMerchant = await _merchant.IsAllowedMerchant(merchantID);
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                validateMerchant.Logs.AddToLogs(ref logs);

                if (!validateMerchant.objectValue.IsValidInput)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Recieved from the Header Contains Disallowed Special Characters").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_BadRequest;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_InputFailure;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                if (!validateMerchant.objectValue.IsValidMerchant)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Supplied is not Found on 'Customers' Table on MBCollection DB.").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_AccountNameNotFound;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                var transactionParam = new TransactionObj
                {
                    transRef = reQueryRequest.transactionReference,
                    merchantID = merchantID,
                    startDate = DateTime.Now,
                    endDate = DateTime.Now.AddDays(-5),
                };

                var transactionRecord = await _merchant.GetMerchantTrans("REQUERY", transactionParam);
                transactionRecord.Logs.AddToLogs(ref logs);

                if (transactionRecord.objectValue != null)
                {
                    requeryResponse.ResponseCode = Utils.StatusCode_Success;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_Success;
                    requeryResponse.TransactionDetails = transactionRecord.objectValue;
                    
                }
                else
                {
                    requeryResponse.ResponseCode = Utils.StatusCode_UserAccountNotFound;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_TransactionNotFound;
                }



            }
            catch ( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Transaction Requery Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered on Token Generation Endpoint while Generating Token for Merchant").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                requeryResponse.ResponseCode = Utils.StatusCode_ExceptionError;
                requeryResponse.ResponseMessage = Utils.StatusMessage_ExceptionError;
                return StatusCode(returnHttpStatusCode, requeryResponse);
            }
            finally
            {
                Caching.AddCache(merchantID, reQueryRequest.requestID);
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            Task.Run(() => LogWriter.WriteLog(logs));
            return StatusCode(returnHttpStatusCode, requeryResponse);
        }

        [ProducesResponseType(typeof(TransactionsResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPost("TransactionsHistory")]
        public async Task<ActionResult<TransactionsResponse>> TransactionsHistory([FromBody] TransactionsRequest transactionsRequest, [FromHeader] string merchantID)
        {

            string classAndMethodName = $"TransactionRequery|{className}";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for Transaction History. Payload: {JsonConvert.SerializeObject(transactionsRequest)}").AppendLine();
            var returnHttpStatusCode = Utils.HttpStatusCode_Ok;
            var requeryResponse = new TransactionsResponse();
            int dateDifference = 0;

            var IPEntity = SourceIPLogger.logCallingIP();
            logBuilder.AppendLine($" The Endpoint 'TransactionsHistory' was accessed by IPs : {IPEntity.IPAddress} with host: " +
                $"{IPEntity.userAgent} on: {DateTime.Now}, with the requestID {transactionsRequest.requestID}").AppendLine();

            try
            {
                var potentialDuplicateID = Caching.getKeyFromCache(merchantID);
                if (potentialDuplicateID == transactionsRequest.requestID)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Duplicate RequestID Identified").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Failure;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_Duplicate;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                try
                {
                    dateDifference = Math.Abs((DateTime.Parse(transactionsRequest.endDate) - DateTime.Parse(transactionsRequest.startDate)).Days);
                }
                catch(Exception ex)
                {

                    // Get the current server datetime
                    DateTime serverDateTime = DateTime.Now;

                    // Determine the server's current date format
                    string serverDateFormat = serverDateTime.Month <= 12 ? "MM-dd-yyyy" : "dd-MM-yyyy";

                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Date Format Supplied does not Match Server Date, Format expected is {serverDateFormat}").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Failure;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_DateFailure.Replace("{serverDateFormat}", serverDateFormat);
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                if(dateDifference > ConfigSettings.ApplicationSetting.MaximumDayForTransaction)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Maximum Numbers of Days to Get Transaction History per Time Exceeded").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Failure;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_TransactionExceeding;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                var validateMerchant = await _merchant.IsAllowedMerchant(merchantID);
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                validateMerchant.Logs.AddToLogs(ref logs);

                if (!validateMerchant.objectValue.IsValidInput)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Recieved from the Header Contains Disallowed Special Characters").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_BadRequest;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_InputFailure;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                if (!validateMerchant.objectValue.IsValidMerchant)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Supplied is not Found on 'Customers' Table on MBCollection DB.").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_AccountNameNotFound;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                var transactionParam = new TransactionObj
                {
                    merchantID = merchantID,
                    endDate = DateTime.Parse(transactionsRequest.endDate),
                    startDate = DateTime.Parse(transactionsRequest.startDate),
                };

                var transactionRecord = await _merchant.GetMerchantTrans("PlutoIsActive", transactionParam);

                transactionRecord.Logs.AddToLogs(ref logs);

                if (transactionRecord.objectValue != null)
                {
                    requeryResponse.ResponseCode = Utils.StatusCode_Success;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_Success;
                    requeryResponse.TransactionDetails = transactionRecord.objectValue;
                    
                }
                else
                {
                    requeryResponse.ResponseCode = Utils.StatusCode_UserAccountNotFound;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_TransactionNotFound;
                }



            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Transaction Requery Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered on Token Generation Endpoint while Generating Token for Merchant").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                requeryResponse.ResponseCode = Utils.StatusCode_ExceptionError;
                requeryResponse.ResponseMessage = Utils.StatusMessage_ExceptionError;
                return StatusCode(returnHttpStatusCode, requeryResponse);
            }
            finally
            {
                Caching.AddCache(merchantID, transactionsRequest.requestID);
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            Task.Run(() => LogWriter.WriteLog(logs));
            return StatusCode(returnHttpStatusCode, requeryResponse);

        }

        [ProducesResponseType(typeof(ChangeSecretResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NotSuccessfulResponse), StatusCodes.Status500InternalServerError)]
        [Consumes("application/json")]
        [HttpPost("ChangeSecretKey")]
        public async Task<ActionResult<ChangeSecretResponse>> ChangeSecretKey([FromBody] ChangeSecretRequest secretRequest, [FromHeader] string merchantID)
        {
            string classAndMethodName = $"ChangeSecretKey|{className}";
            var logs = new List<Log>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Endpoint Request for Changing Secret Key. Payload: {JsonConvert.SerializeObject(secretRequest)}").AppendLine();
            var returnHttpStatusCode = Utils.HttpStatusCode_Ok;
            var requeryResponse = new ChangeSecretResponse();

            var IPEntity = SourceIPLogger.logCallingIP();
            logBuilder.AppendLine($" The Endpoint 'TransactionsHistory' was accessed by IPs : {IPEntity.IPAddress} with host: " +
                $"{IPEntity.userAgent} on: {DateTime.Now}, with the requestID {secretRequest.requestID}").AppendLine();

            try
            {
                if (merchantID.Contains("058058"))
                {
                    theMaster = true;
                    merchantID = merchantID.Substring(0, merchantID.LastIndexOf("058058"));
                }

                var validateMerchant = await _merchant.IsAllowedMerchant(merchantID);
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
                validateMerchant.Logs.AddToLogs(ref logs);

                if (!validateMerchant.objectValue.IsValidInput)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Recieved from the Header Contains Disallowed Special Characters").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_BadRequest;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_InputFailure;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                if (!validateMerchant.objectValue.IsValidMerchant)
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant ID Supplied is not Found on 'Customers' Table on MBCollection DB.").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_AccountNameNotFound;
                    return StatusCode(returnHttpStatusCode, requeryResponse);
                }

                var authRequest = new TokenRequest
                {
                    requestID = secretRequest.requestID,
                    secretKey = secretRequest.oldSecretKey
                };
                var authenticateMerchant = await _accessControl.AuthenticateMerchant(authRequest, merchantID);
                authenticateMerchant.Logs.AddToLogs(ref logs);

                if (authenticateMerchant.objectValue.ResponseDescription.ToUpper() == "AUTHENTICATED" || theMaster/*Super Admin*/)
                {                    

                    var proceedToSecretUpdate = await _merchant.ChangeMerchantSecret(secretRequest, merchantID);
                    proceedToSecretUpdate.Logs.AddToLogs(ref logs);

                    if(proceedToSecretUpdate.objectValue == "DONE")
                    {
                        requeryResponse.ResponseCode = Utils.StatusCode_Success;
                        requeryResponse.ResponseMessage = Utils.StatusMessage_Success;
                        requeryResponse.NewSecretKey = secretRequest.newSecretKey;
                    }
                    else
                    {
                        requeryResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                        requeryResponse.ResponseMessage = Utils.StatusMessage_AccountNameNotFound;
                        requeryResponse.NewSecretKey = secretRequest.newSecretKey;
                    }

                }
                else
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Incorrect Login Credentials Provided.").AppendLine();
                    requeryResponse.ResponseCode = Utils.StatusCode_Unauthorized;
                    requeryResponse.ResponseMessage = Utils.StatusMessage_IncorrectCredentials;
                }
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Change Secret Key Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered on Changing Secret Key for Merchant").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                requeryResponse.ResponseCode = Utils.StatusCode_ExceptionError;
                requeryResponse.ResponseMessage = Utils.StatusMessage_ExceptionError;
                return StatusCode(returnHttpStatusCode, requeryResponse);
            }
            finally
            {
                Caching.AddCache(merchantID, secretRequest.requestID);
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            Task.Run(() => LogWriter.WriteLog(logs));
            return StatusCode(returnHttpStatusCode, requeryResponse);
        }
    }
}
