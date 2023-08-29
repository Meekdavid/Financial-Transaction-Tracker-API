using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Common
{
    public class DatabaseStoredProcedures
    {
        public const string Proc_ValidateMerchant = "[dbo].[ValidateMerchant]";
        public const string Proc_AuthenticateMerchant = "[dbo].[AuthenticateMerchant]";
        public const string Proc_MerchantDetails = "[dbo].[SelectMerchantDetails]";
        public const string Proc_MerchantTransactions = "[dbo].[SelectMerchantTransactions]";
        public const string Proc_UpdateMerchantSecret = "[dbo].[UpdateMerchantSecret]";
    }
}
