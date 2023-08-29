using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Dtos.Models;
using MerchantTransactionCore.Helpers.Common.ORM;
using MerchantTransactionCore.Helpers.Common;
using MerchantTransactionCore.Helpers.ConfigurationSettings.ConfigManager;
using MerchantTransactionCore.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MerchantTransactionCore.Helpers.Extensions;
using MerchantTransactionCore.Helpers.Logger;
using static MerchantTransactionCore.Helpers.Common.Utils;
using System.Data.SqlClient;
using Dapper;
using System.Text.RegularExpressions;
using MerchantTransactionCore.Dtos.ReQuery;
using System.Diagnostics.Metrics;
using System.Transactions;
using MerchantTransactionCore.Dtos.ChangeSecret;

namespace MerchantTransactionCore.Repositories
{
    public class Merchant : IMerchant
    {
        private string className = string.Empty;
        public Merchant()
        {
            className = GetType().Name;
        }
        public async Task<MethodReturnResponse<MerchantDetails>> GetMerchantDetails(string merchantId)
        {
            string methodName = "GetMerchantDetails", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            MerchantDetails theReturner = new MerchantDetails();
            int counter = 0;
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Retreive Merchant Details with Merchant ID: {merchantId}").AppendLine();

            try
            {
                string query = DatabaseStoredProcedures.Proc_MerchantDetails;
                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {

                    var paras = new Dictionary<string, DapperParameterWrapper>();
                    paras.Add("@customerID", new DapperParameterWrapper(string.IsNullOrWhiteSpace(merchantId) ? string.Empty : merchantId.Trim(), DbType.String));

                    DataTable storedProcedureReturnResponse = await DapperUtility<DataTable>.GetTableAsync(sqlConnection, paras, query, CommandType.StoredProcedure);

                    if (storedProcedureReturnResponse?.Rows.Count > 0)
                    {
                        theReturner.Merchant = storedProcedureReturnResponse.Rows[0][0].ToString();
                        var merchantSub = new List<string>();

                        for (counter = 0; counter < storedProcedureReturnResponse.Rows.Count; counter++)
                        {
                            merchantSub.Add(storedProcedureReturnResponse.Rows[counter]["form_title"].ToString());
                        }

                        theReturner.Subsidaries = merchantSub.ToArray();
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant Details Sucessfully Retreived").AppendLine();

                    }

                }
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Retreive Merchant Details Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Retreiving Merchant Details").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                if (ex.GetType().Equals(typeof(SqlException)))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Time-Out Error was Encountered, while Connecting to the Database").AppendLine();
                    return new MethodReturnResponse<MerchantDetails>
                    {
                        Logs = logs,
                        objectValue = theReturner
                    };
                }

                return new MethodReturnResponse<MerchantDetails>
                {
                    Logs = logs,
                    objectValue = theReturner
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }
            return new MethodReturnResponse<MerchantDetails>
            {
                Logs =logs,
                objectValue = theReturner
            };
        }

        public async Task<MethodReturnResponse<object>> GetMerchantTrans(string flag, TransactionObj paramValues)
        {
            string methodName = "GetMerchantTransactions", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();            

            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Retreive Merchant Transactions").AppendLine();

            try
            {
                string query = DatabaseStoredProcedures.Proc_MerchantTransactions;
                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {

                    var paras = new Dictionary<string, DapperParameterWrapper>();
                    paras.Add("@reference", new DapperParameterWrapper(string.IsNullOrWhiteSpace(paramValues.transRef) ? string.Empty : paramValues.transRef.Trim(), DbType.String));
                    paras.Add("@customerID", new DapperParameterWrapper(string.IsNullOrWhiteSpace(paramValues.merchantID) ? string.Empty : paramValues.merchantID.Trim(), DbType.String));
                    paras.Add("@flag", new DapperParameterWrapper(string.IsNullOrWhiteSpace(flag) ? string.Empty : flag.Trim(), DbType.String));
                    paras.Add("@beginDate", new DapperParameterWrapper(paramValues.startDate.Equals(DateTime.MinValue) ? DateTime.MinValue : paramValues.startDate, DbType.DateTime));
                    paras.Add("@endDate", new DapperParameterWrapper(paramValues.endDate.Equals(DateTime.MinValue) ? DateTime.MinValue : paramValues.endDate, DbType.DateTime));

                    DataTable storedProcedureReturnResponse = await DapperUtility<DataTable>.GetTableAsync(sqlConnection, paras, query, CommandType.StoredProcedure);

                    if (storedProcedureReturnResponse?.Rows.Count > 0)
                    {
                        if (storedProcedureReturnResponse?.Rows.Count == 1)
                        {
                            var theReturner = new TransactionDetails();
                            foreach (DataRow row in storedProcedureReturnResponse.Rows)
                            {
                                theReturner.TransactionId = row["reference"].ToString();
                                theReturner.TransactionDate = row["trans_date"].ToString();
                                theReturner.Narration = row["narration"].ToString();
                                theReturner.Amount = $"₦{decimal.Parse(row["tra_amt_mert"].ToString()).ToString("N2")}";
                            }
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant Transactions Sucessfully Retreived").AppendLine();

                            return new MethodReturnResponse<object>()
                            {
                                Logs = logs,
                                objectValue = theReturner
                            };
                        }
                        else
                        {
                            var theReturner = new List<TransactionDetails>();
                            foreach (DataRow row in storedProcedureReturnResponse.Rows)
                            {

                                var holder = new TransactionDetails
                                {
                                    TransactionId = row["reference"].ToString(),
                                    TransactionDate = row["trans_date"].ToString(),
                                    Narration = row["narration"].ToString(),
                                    Amount = $"₦{decimal.Parse(row["tra_amt_mert"].ToString()).ToString("N2")}"
                                };

                                theReturner.Add(holder);
                            }
                            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant Transactions Sucessfully Retreived").AppendLine();
                            return new MethodReturnResponse<object>()
                            {
                                Logs = logs,
                                objectValue = theReturner.ToArray()
                            };
                        }

                    }
                    else { logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss}  No Transaction Records Found for Merchant").AppendLine(); }

                }
            }
            catch ( Exception ex )
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Retreive Merchant Transaction Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Retreiving Merchant Transaction").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                if (ex.GetType().Equals(typeof(SqlException)))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Time-Out Error was Encountered, while Connecting to the Database").AppendLine();
                    return new MethodReturnResponse<object>
                    {
                        Logs = logs
                        
                    };
                }

                return new MethodReturnResponse<object>
                {
                    Logs = logs,
                    objectValue = null
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return new MethodReturnResponse<object>
            {
                Logs = logs,
                objectValue = null
            };
        }

        public async Task<MethodReturnResponse<ValidateMerchant>> IsAllowedMerchant(string merchantId)
        {
            string methodName = "ValidateMerchant", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            ValidateMerchant theReturner = new ValidateMerchant();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Validate Merchant Details with Merchant ID: {merchantId}").AppendLine();

            try
            {
                string pattern = @"[<>.*?&'$=]|(\bOR\b)";
                if (!Regex.IsMatch(merchantId, pattern, RegexOptions.IgnoreCase))
                {
                    theReturner.IsValidInput = true;
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Input Supplied Sucessfully Passed XML Injection Checks").AppendLine();
                }

                string query = DatabaseStoredProcedures.Proc_ValidateMerchant;
                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {
                    DynamicParameters paras = new DynamicParameters();
                    paras.Add("@customer_id", string.IsNullOrWhiteSpace(merchantId) ? string.Empty : merchantId.Trim());

                    var storedProcedureReturnResponse = SqlMapper.Query<StoredProcedureReturnResponse<string>>(sqlConnection, query, paras, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if (storedProcedureReturnResponse?._Message?.ToUpper() == "EXIST")
                    {
                        theReturner.IsValidMerchant = true;
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant Details have been Validated and it's Record Exists on the DB").AppendLine();
                    }

                }
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Retreive Merchant Details Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Retreiving Merchant Details").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                if (ex.GetType().Equals(typeof(SqlException)))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Time-Out Error was Encountered, while Connecting to the Database").AppendLine();
                    return new MethodReturnResponse<ValidateMerchant>
                    {
                        Logs = logs,
                        objectValue = theReturner
                    };
                }

                return new MethodReturnResponse<ValidateMerchant>
                {
                    Logs = logs,
                    objectValue = theReturner
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return new MethodReturnResponse<ValidateMerchant>
            {
                Logs = logs,
                objectValue = theReturner,
            };
        }

        public async Task<MethodReturnResponse<string>> ChangeMerchantSecret(ChangeSecretRequest theSecret, string merchantId)
        {
            string methodName = "ChangeMerchantSecret", classAndMethodName = $"{className}.{methodName}";

            var logs = new List<Log>();
            var theReturner = new MethodReturnResponse<string>();
            var logBuilder = new StringBuilder($"--------------{classAndMethodName}--------START--------").AppendLine();
            logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Received Request to Validate Merchant Details with Merchant ID: {merchantId}").AppendLine();

            try
            {
                
                string query = DatabaseStoredProcedures.Proc_UpdateMerchantSecret;
                using (var sqlConnection = ConnectionManager.SqlDatabaseCreateConnection(ConfigSettings.ConnectionString.MBCollectionConnectionString))
                {
                    DynamicParameters paras = new DynamicParameters();
                    paras.Add("@customer_id", string.IsNullOrWhiteSpace(merchantId) ? string.Empty : merchantId.Trim());
                    paras.Add("@authKey", string.IsNullOrWhiteSpace(theSecret.newSecretKey) ? string.Empty : theSecret.newSecretKey.Trim());

                    var storedProcedureReturnResponse = SqlMapper.Query<StoredProcedureReturnResponse<string>>(sqlConnection, query, paras, commandType: CommandType.StoredProcedure).FirstOrDefault();

                    if (storedProcedureReturnResponse?._Message?.ToUpper() == "DONE")
                    {
                        theReturner.objectValue = "DONE";
                        theReturner.Logs = logs;
                        logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Merchant Credentials Sucessfully Updated").AppendLine();
                    }

                }
            }
            catch (Exception ex)
            {
                //ON EXCEPTION STORE THE PREVIOUS LOG
                LogWriter.AddLogAndClearLogBuilderOnException(ref logBuilder, LoMBype.LOG_DEBUG, ref logs, ex, "Update Merchant Secret Exception");

                logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Error Encountered while Updating Merchant Secret").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);

                if (ex.GetType().Equals(typeof(SqlException)))
                {
                    logBuilder.AppendLine($"{DateTime.Now:dd-MM-yyyy HH:mm:ss} Time-Out Error was Encountered, while Connecting to the Database").AppendLine();
                    return new MethodReturnResponse<string>
                    {
                        Logs = logs,
                        objectValue = null
                    };
                }

                return new MethodReturnResponse<string>
                {
                    Logs = logs,
                    objectValue = null
                };
            }
            finally
            {
                logBuilder.AppendLine($"--------------{classAndMethodName}--------END--------").AppendLine();
                logBuilder.ToString().AddToLogs(ref logs);
                logBuilder.Clear();
            }

            return theReturner;
        }
    }
}
