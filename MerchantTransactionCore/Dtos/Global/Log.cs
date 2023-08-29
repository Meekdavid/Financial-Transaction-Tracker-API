using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Global
{
    public class Log
    {
        public string MessageLog { get; set; }
        public int LoMBype { get; set; }
        public Exception ExceptionLog { get; set; } = null;
    }
}
