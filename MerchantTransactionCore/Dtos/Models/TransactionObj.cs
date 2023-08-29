using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class TransactionObj
    {
        public string transRef { get; set; } 
        public string merchantID { get; set; }
        public DateTime startDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
