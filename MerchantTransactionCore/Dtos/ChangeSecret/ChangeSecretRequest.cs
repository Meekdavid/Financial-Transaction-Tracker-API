using MerchantTransactionCore.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.ChangeSecret
{
    public class ChangeSecretRequest
    {
        [Validate]
        public string requestID { get; set; }        
        [Validate]
        public string oldSecretKey { get; set; }
        [Validate]
        public string newSecretKey { get; set; }
    }
}
