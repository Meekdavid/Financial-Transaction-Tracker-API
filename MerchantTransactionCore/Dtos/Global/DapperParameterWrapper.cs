using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Global
{
    public class DapperParameterWrapper
    {
        public object ParameterValue { get; set; }
        public DbType DbParameterType { get; set; }
        public ParameterDirection DbParameterDirection { get; set; } = ParameterDirection.Input;
        public int size { get; set; }

        public DapperParameterWrapper(object _ParameterValue, DbType _DbParameterType, int _size = default, ParameterDirection _DbParameterDirection = ParameterDirection.Input)
        {
            ParameterValue = _ParameterValue;
            DbParameterType = _DbParameterType;
            DbParameterDirection = _DbParameterDirection;
            size = _size;
        }
    }
}
