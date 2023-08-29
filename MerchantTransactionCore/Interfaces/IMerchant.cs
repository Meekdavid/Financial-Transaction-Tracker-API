using MerchantTransactionCore.Dtos.ChangeSecret;
using MerchantTransactionCore.Dtos.Global;
using MerchantTransactionCore.Dtos.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Interfaces
{
    public interface IMerchant
    {
        Task<MethodReturnResponse<ValidateMerchant>> IsAllowedMerchant(string merchantId);
        Task<MethodReturnResponse<MerchantDetails>> GetMerchantDetails(string merchantId);
        Task<MethodReturnResponse<object>> GetMerchantTrans(string flag, TransactionObj paramValues);
        Task<MethodReturnResponse<string>> ChangeMerchantSecret(ChangeSecretRequest theSecret, string merchantId);
    }
}
