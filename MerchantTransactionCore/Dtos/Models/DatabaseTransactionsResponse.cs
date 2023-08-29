using MerchantTransactionCore.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Models
{
    public class DatabaseTransactionsResponse : StoredProcedureReturnResponse<DataTable>
    {
    }
}
