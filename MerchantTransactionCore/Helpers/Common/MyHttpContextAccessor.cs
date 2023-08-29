using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Helpers.Common
{
    public static class MyHttpContextAccessor
    {
        private static IHttpContextAccessor _HttpContextAccessor;

        public static IHttpContextAccessor HttpContextAccessor
        {
            set
            {
                _HttpContextAccessor = value;
            }
        }

        public static IHttpContextAccessor GetHttpContextAccessor()
        {
            return _HttpContextAccessor;
        }
    }
}
