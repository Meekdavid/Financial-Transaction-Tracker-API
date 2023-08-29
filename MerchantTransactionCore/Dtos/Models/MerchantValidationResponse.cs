using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class MerchantValidationResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseDescription { get; set; }
    }
}
