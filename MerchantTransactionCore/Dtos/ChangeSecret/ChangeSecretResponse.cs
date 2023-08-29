using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.ChangeSecret
{
    public class ChangeSecretResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string NewSecretKey { get; set; }
    }
}
