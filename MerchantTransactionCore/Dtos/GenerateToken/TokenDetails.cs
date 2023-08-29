using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.GenerateToken
{
    public class TokenDetails
    {
        public string Token { get; set; }
        public string ExpiresIn { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string TokenType { get; set; }
    }
}
