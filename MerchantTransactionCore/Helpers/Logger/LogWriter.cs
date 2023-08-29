using MerchantTransactionCore.Dtos.Global;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MerchantTransactionCore.Helpers.Common.Utils;

namespace MerchantTransactionCore.Helpers.Logger
{
    public static class LogWriter
    {
        private static ILogger _Logger;

        public static ILogger Logger
        {
            set
            {
                _Logger = value;
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="logs">List of Log Objects to write to Sink.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(List<Log> logs)
        {
            foreach (var log in logs)
            {
                MainLogWriter(log.MessageLog, (LoMBype)log.LoMBype, log.ExceptionLog);
            }
        }

        /// <summary>
        /// Write Log to the Configured Sink
        /// </summary>
        /// <param name="messageLog">Message Log as Text</param>
        /// <param name="loMBype">Logging Type to Use</param>
        /// <param name="exceptionLog">Exception Object if any.</param>
        /// <returns>Initiated Token Details</returns>
        /// <response code="200">Returns the Initiated Token Details</response>
        public static void WriteLog(string messageLog, LoMBype loMBype, Exception exceptionLog = null)
        {
            MainLogWriter(messageLog, loMBype, exceptionLog);
        }

        private static void MainLogWriter(string messageLog, LoMBype loMBype, Exception exceptionLog = null)
        {
            try
            {
                switch (loMBype)
                {
                    case LoMBype.LOG_DEBUG:

                        _Logger.LogDebug(exceptionLog, messageLog);

                        break;
                    case LoMBype.LOG_INFORMATION:

                        _Logger.LogInformation(messageLog);

                        break;
                    case LoMBype.LOG_ERROR:

                        _Logger.LogError(messageLog);

                        break;
                    default:
                        //DO NOTHING
                        break;
                }
            }
            catch (Exception ex)
            {
                var eventLog = new EventLog();
                eventLog.Source = "MBCollectionRequeryAPI";
                //eventLog.WriteEntry("Error in: " + MyHttpContextAccessor.GetHttpContext()?.Request?.GetEncodedUrl());
                eventLog.WriteEntry(ex.Message, EventLogEntryType.Information);
            }
        }

        //[Obsolete]
        public static void AddLogAndClearLogBuilderOnException(ref StringBuilder logBuilder, LoMBype loMBype, ref List<Log> logs, Exception exception, string exceptionMessage = null)
        {
            logs.Add(new Log()
            {
                LoMBype = (int)loMBype,
                MessageLog = logBuilder.ToString()
            });
            logBuilder.Clear();

            logs.Add(new Log()
            {
                LoMBype = (int)LoMBype.LOG_DEBUG,
                MessageLog = exceptionMessage,
                ExceptionLog = exception
            });
        }
    }
}
