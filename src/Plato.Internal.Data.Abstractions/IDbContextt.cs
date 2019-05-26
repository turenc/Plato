﻿using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Plato.Internal.Data.Abstractions
{
    public interface IDbContext : IDisposable
    {

        void Configure(Action<DbContextOptions> options);
   
        DbContextOptions Configuration { get; }

        //Task<T> ExecuteReaderAsync<T>(CommandType commandType, string sql, Func<DbDataReader, Task<T>> populate, params object[] args) where T : class;

        //Task<T> ExecuteScalarAsync<T>(CommandType commandType, string sql, params object[] commandParams);
   
        //Task<T> ExecuteNonQueryAsync<T>(CommandType commandType, string sql, params object[] commandParams);
        
        // Testing
        
        Task<T> ExecuteReaderAsync2<T>(CommandType commandType, string sql, Func<DbDataReader, Task<T>> populate, DbParam[] dbParams = null) where T : class;

        Task<T> ExecuteScalarAsync2<T>(CommandType commandType, string sql, DbParam[] dbParams = null);

        Task<T> ExecuteNonQueryAsync2<T>(CommandType commandType, string sql, DbParam[] dbParams = null);




    }

}
