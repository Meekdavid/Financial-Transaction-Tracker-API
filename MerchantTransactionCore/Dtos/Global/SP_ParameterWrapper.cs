using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MerchantTransactionCore.Dtos.Global
{
    public class SP_ParameterWrapper
    {
        /// <summary>
        /// For Input Parameters
        /// </summary>
        /// <param name="_ParameterValue"></param>
        /// <param name="_DbParameterType"></param>
        /// <param name="_DbParameterDirection"></param>
        public SP_ParameterWrapper(object _ParameterValue, DbType _DbParameterType = DbType.String, int _size = default, ParameterDirection _DbParameterDirection = ParameterDirection.Input)
        {
            ParameterValue = _ParameterValue;
            DbParameterType = _DbParameterType;
            DbParameterDirection = _DbParameterDirection;
            size = _size;
        }

        /// <summary>
        /// For Output Parameters
        /// </summary>
        /// <param name="_DbParameterType"></param>
        /// <param name="_DbParameterDirection"></param>
        public SP_ParameterWrapper(DbType _DbParameterType = DbType.String, int _size = default, ParameterDirection _DbParameterDirection = ParameterDirection.Output)
        {
            DbParameterType = _DbParameterType;
            DbParameterDirection = _DbParameterDirection;
            size = _size;
        }

        public object ParameterValue { get; set; }
        public DbType DbParameterType { get; set; } = DbType.String;
        public ParameterDirection DbParameterDirection { get; set; } = ParameterDirection.Input;
        public int size { get; set; }
    }
}
