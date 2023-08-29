using MerchantTransactionCore.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.FetchTransactions
{
    public class TransactionsRequest
    {
        [Validate]
        public string requestID { get; set; }
        [Validate]
        public string startDate { get; set; }
        [Validate]
        public string endDate { get; set; }
        //TO-DO:: Implement validation for date difference
        
    }
}
