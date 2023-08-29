using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class IPModel
    {
        public string IPAddress { get; set; }
        public string userAgent { get; set; }
        public string exception { get; set; }
    }
}
