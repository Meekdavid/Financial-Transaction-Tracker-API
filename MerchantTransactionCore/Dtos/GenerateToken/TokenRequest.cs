using MerchantTransactionCore.Helpers.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.GenerateToken
{
    public class TokenRequest
    {
        [Validate]
        public string requestID { get; set; }
        
        [Validate]
        public string secretKey { get; set; }
    }
}
