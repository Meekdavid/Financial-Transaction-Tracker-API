using MerchantTransactionCore.Dtos.ReQuery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.FetchTransactions
{
    public class TransactionsResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        //public TransactionDetails[] TransactionDetails { get; set; }
        public Object TransactionDetails { get; set; }
    }
}
