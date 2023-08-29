using MerchantTransactionCore.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MerchantTransactionCore.Helpers.Common.Utils;

namespace MerchantTransactionCore.Helpers.Extensions
{
    public static class StringExtensions
    {
        public static string ToDictionaryString<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : class where TValue : class
        {
            return $"{{ {string.Join(", ", dictionary.Select(kv => kv.Key + " = " + kv.Value).ToArray())} }}";
        }

        
        public static void AddToLogs(this string messageLog, ref List<Log> logs, LoMBype loMBype = LoMBype.LOG_DEBUG, Exception exceptionLog = null)
        {
            logs.Add(new Log()
            {
                MessageLog = messageLog,
                LoMBype = (int)loMBype,
                ExceptionLog = exceptionLog
            });
        }
    }
}
