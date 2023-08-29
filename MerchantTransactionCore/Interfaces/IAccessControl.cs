using MerchantTransactionCore.Dtos.GenerateToken;
using MerchantTransactionCore.Dtos.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Interfaces
{
    public interface IAccessControl
    {
        Task<MethodReturnResponse<GetTokenResponse>> GetAccessToken(TokenRequest tokenRequest, string merchantID);
        Task<MethodReturnResponse<MerchantValidationResponse>> AuthenticateMerchant(TokenRequest tokenRequest, string merchantID);
    }
}
