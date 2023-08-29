using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Global
{
    public class StoredProcedureReturnResponse<T>
    {
        /// <summary>
        /// Response Code
        /// </summary>
        public virtual string _Code { get; set; }
        /// <summary>
        /// Database Entity
        /// </summary>
        public virtual T? _Message { get; set; }
        /// <summary>
        /// Success Indicator
        /// </summary>
        public virtual bool _Success { get; set; }
    }
}
