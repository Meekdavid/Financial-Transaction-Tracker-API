using MerchantTransactionCore.Dtos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.GenerateToken
{
    public class TokenResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }        
        public TokenDetails TokenDetails { get; set; }
        public MerchantDetails MerchantDetails { get; set; }
    }
}
