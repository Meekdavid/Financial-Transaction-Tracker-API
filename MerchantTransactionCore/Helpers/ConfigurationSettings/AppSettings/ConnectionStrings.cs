using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.ConfigurationSettings.AppSettings
{
    public class ConnectionStrings
    {
        private string _BasisConnectionString { get; set; }
        public string BasisConnectionString
        {
            get
            {
                return MBBEncryptLibrary.MBBEncryptLib.DecryptText(_BasisConnectionString);
            }
            set
            {
                _BasisConnectionString = value;
            }
        }

        private string _MBCollectionConnectionString { get; set; }
        public string MBCollectionConnectionString
        {
            get
            {
                return MBBEncryptLibrary.MBBEncryptLib.DecryptText(_MBCollectionConnectionString);
            }
            set
            {
                _MBCollectionConnectionString = value;
            }
        }

    }
}
