using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.ConfigurationSettings.AppSettings
{
    public class ApplicationSettings
    {
        public string ValidateSourceIP { get; set; }
        public string AllowedIPs { get; set; }
        public int ResponseCacheTime { get; set; }
        public int OtherServicesCacheTime { get; set; }
        public bool ActivateResponseCaching { get; set; }
        public int MaximumDayForTransaction { get; set; }
    }
}
