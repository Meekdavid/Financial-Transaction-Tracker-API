using MBBEncryptLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.ConfigurationSettings.AppSettings
{
    public class JWTConfig
    {
        public string _PrivateKey { get; set; }
        public string PrivateKey
        {
            get
            {
                return MBBEncryptLib.DecryptText(_PrivateKey);
            }
            set
            {
                _PrivateKey = value;
            }
        }

        public double TokenExpiration { get; set; }
    }
}
