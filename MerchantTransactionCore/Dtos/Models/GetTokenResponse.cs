using MerchantTransactionCore.Dtos.GenerateToken;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class GetTokenResponse
    {
        public TokenDetails? AccessToken { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }
}
