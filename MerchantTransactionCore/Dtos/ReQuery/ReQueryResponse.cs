using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.ReQuery
{
    public class ReQueryResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public object TransactionDetails { get; set; }
    }

    public class TransactionDetails
    {
        public string TransactionId { get; set;}
        public string TransactionDate { get; set;}
        public string Narration { get; set;}
        public string Amount { get; set;}
    }
}
