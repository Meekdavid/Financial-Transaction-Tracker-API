using MerchantTransactionCore.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.ReQuery
{
    public class ReQueryRequest
    {
        [Validate]
        public string requestID { get; set; }
        [Validate]
        public string transactionReference { get; set; }
    }
}
