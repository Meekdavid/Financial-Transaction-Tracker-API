using MerchantTransactionCore.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class MethodReturnResponse<T> where T : class
    {
        public T? objectValue { get; set; }        
        public List<Log> Logs { get; set; }
    }
}
