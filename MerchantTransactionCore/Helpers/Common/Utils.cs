using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Common
{
    public class Utils
    {
        /***** STATUS CODES *****/
        public const string StatusCode_Success = "00";
        public const string StatusCode_UserAccountNotFound = "01";
        public const string StatusCode_TokenNullValue = "02";        
        public const string StatusCode_BadRequest = "03";
        public const string StatusCode_Unauthorized = "04";
        public const string StatusCode_PartialContent = "05";
        public const string StatusCode_Failure = "06";
        public const string StatusCode_DatabaseConnectionTimeout = "07";
        public const string StatusCode_StoredProcedureError = "08";
        public const string StatusCode_ExceptionError = "09";
        public const string StatusCode_DatabaseConnectionError = "10";

        /***** STATUS MESSAGES *****/
        public const string StatusMessage_Success = "Request Successful.";        
        public const string StatusMessage_Failure = "Request Failed";
        public const string StatusMessage_Duplicate = "Failed: Duplicate RequestID";
        public const string StatusMessage_UnknownError = "Unknown Error Occured while Performing this Action.";
        public const string StatusMessage_TokenNullValue = "Authorization Token Value is Null";
        public const string StatusMessage_BadRequest = "Required request parameter is Invalid / Missing";
        public const string StatusMessage_Unauthorized = "Authentication Token is Unauthorized";
        public const string StatusMessage_DatabaseConnectionTimeout = "Database Connection Timeout";
        public const string StatusMessage_StoredProcedureError = "Stored Procedured Execution Failed";
        public const string StatusMessage_ExceptionError = "An Exception Occured";
        public const string StatusMessage_DatabaseConnectionError = "Database Connection Error";
        public const string StatusMessage_AccountNameNotFound = "Merchant Details Not Found";
        public const string StatusMessage_TransactionNotFound = "Transaction Information Not Found";
        public const string StatusMessage_AuthenticationFailue = "Unable to Authenticate Merchant, Please Try Again Later!";
        public const string StatusMessage_InputFailure = "Header Value 'Merchant ID' Contains Disallowed Special Characters";
        public const string StatusMessage_DateFailure = "Date Format Supplied does not Match Server Date, Format expected is {serverDateFormat}";
        public const string StatusMessage_TransactionExceeding = "Maximumn Number of Days to Request Transaction History Per Time Exceeded";
        public const string StatusMessage_IncorrectCredentials = "Incorrect Login Credentials Provided";


        /***** LOG TYPES *****/
        public enum LoMBype
        {
            /// <summary>
            /// Log Message in Debug Level
            /// </summary>
            [Description("Log Message in Debug Level")]
            LOG_DEBUG = 1,
            /// <summary>
            /// Log Message in Information Level
            /// </summary>
            [Description("Log Message in Information Level")]
            LOG_INFORMATION = 2,
            /// <summary>
            /// Log Message in Error Level
            /// </summary>
            [Description("Log Message in Error Level")]
            LOG_ERROR = 3
        }

        //APPLICATION HTTP STATUS CODES
        public const int HttpStatusCode_Ok = StatusCodes.Status200OK;
        public const int HttpStatusCode_BadRequest = StatusCodes.Status400BadRequest;
    }
}
