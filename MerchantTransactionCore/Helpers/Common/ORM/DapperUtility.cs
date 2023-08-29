using Dapper;
using MerchantTransactionCore.Dtos.Global;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DapperExtensions;

namespace MerchantTransactionCore.Helpers.Common.ORM
{
    public class DapperUtility<T>
    {
        static IDbConnection _dbConnection;

        public static async Task<T> GetSingleObjectAsync(IDbConnection dbConnection, IDictionary<string, SP_ParameterWrapper> paras, string procName, CommandType commandType)
        {
            T retVal;
            try
            {
                _dbConnection = dbConnection;

                using (var con = _dbConnection)
                {
                    DynamicParameters dbParams = new DynamicParameters();
                    foreach (var para in paras)
                    {
                        var dictionaryValue = para.Value;
                        if (dictionaryValue.DbParameterDirection == ParameterDirection.Output)
                        {
                            dbParams.Add(para.Key, dbType: dictionaryValue.DbParameterType, direction: dictionaryValue.DbParameterDirection, size: dictionaryValue.size);
                        }
                        else
                        {
                            dbParams.Add(para.Key, dictionaryValue.ParameterValue, dictionaryValue.DbParameterType, dictionaryValue.DbParameterDirection, size: dictionaryValue.size);
                        }
                    }

                    retVal = await SqlMapper.QueryFirstOrDefaultAsync<T>(con, procName, dbParams, commandType: commandType);
                }
                return retVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<T> GetObjectAsync(IDbConnection dbConnection, IDictionary<string, SP_ParameterWrapper> paras, string procName, CommandType commandType)
        {
            try
            {
                _dbConnection = dbConnection;
                T retVal;
                using (var con = _dbConnection)
                {
                    DynamicParameters dbParams = new DynamicParameters();
                    foreach (var para in paras)
                    {
                        var dictionaryValue = para.Value;
                        if (dictionaryValue.DbParameterDirection == ParameterDirection.Output)
                        {
                            dbParams.Add(para.Key, dbType: dictionaryValue.DbParameterType, direction: dictionaryValue.DbParameterDirection);
                        }
                        else
                        {
                            dbParams.Add(para.Key, dictionaryValue.ParameterValue, dictionaryValue.DbParameterType, dictionaryValue.DbParameterDirection);
                        }
                    }
                    retVal = await SqlMapper.QueryFirstOrDefaultAsync<T>(con, procName, dbParams, commandType: commandType);
                }
                return retVal;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<DataTable> GetTableAsync(IDbConnection dbConnection, IDictionary<string, DapperParameterWrapper> paras, string procName, CommandType commandType, bool basisPosting = default)
        {
            try
            {
                _dbConnection = dbConnection;
                DataTable dataTable = new DataTable();
                using (var con = _dbConnection)
                {
                    DynamicParameters dbParams = new DynamicParameters();
                    foreach (var para in paras)
                    {
                        var dictionaryValue = para.Value;
                        if (dictionaryValue.DbParameterDirection == ParameterDirection.Output)
                        {
                            dbParams.Add(para.Key, dbType: dictionaryValue.DbParameterType, direction: dictionaryValue.DbParameterDirection, size: dictionaryValue.size);
                        }
                        else
                        {
                            dbParams.Add(para.Key, dictionaryValue.ParameterValue, dictionaryValue.DbParameterType, dictionaryValue.DbParameterDirection, size: dictionaryValue.size);
                        }
                    }
                    var Results = await SqlMapper.QueryAsync(con, procName, dbParams, commandType: commandType);

                    //Tried Casting 'Results' to Generic, but it throwed an error, I resulted to returning dataTable;
                    if (Results != null && Results.Any())
                    {
                        var firstResult = Results.First();
                        var properties = ((IDictionary<string, object>)firstResult).Keys;

                        foreach (var property in properties)
                            dataTable.Columns.Add(property);

                        foreach (var result in Results)
                        {
                            var dataRow = dataTable.NewRow();

                            foreach (var property in properties)
                                dataRow[property] = ((IDictionary<string, object>)result)[property];

                            dataTable.Rows.Add(dataRow);
                        }
                    }
                }
                return dataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


    }
}
