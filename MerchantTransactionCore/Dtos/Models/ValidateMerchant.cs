using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class ValidateMerchant
    {
        public bool IsValidInput { get; set; } = false;
        public bool IsValidMerchant { get; set; } = false;
    }
}
