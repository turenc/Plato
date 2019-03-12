﻿using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;
using Plato.Internal.Models.Users;

namespace Plato.Internal.Repositories.Users
{
    public class UserSecretRepository : IUserSecretRepository<UserSecret>
    {
        #region "Constructor"

        public UserSecretRepository(
            IDbContext dbContext,
            ILogger<UserSecretRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        #endregion

        #region "Private Variables"

        private readonly IDbContext _dbContext;
        private readonly ILogger<UserSecretRepository> _logger;

        #endregion

        #region "Implementation"
        
        public Task<bool> DeleteAsync(int id)
        {
            // TODO
            throw new NotImplementedException();
        }

        public async Task<UserSecret> InsertUpdateAsync(UserSecret secret)
        {
            var id = await InsertUpdateInternal(
                secret.Id,
                secret.UserId,
                secret.Secret,
                secret.Salts,
                secret.SecurityStamp);

            if (id > 0)
                return await SelectByIdAsync(id);

            return null;
        }

        public async Task<UserSecret> SelectByIdAsync(int id)
        {
            UserSecret secret = null;
            using (var context = _dbContext)
            {
                secret = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "plato_sp_SelectUserSecret",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            secret = new UserSecret();
                            await reader.ReadAsync();
                            secret.PopulateModel(reader);
                        }

                        return secret;
                    },
                    id);

              
            }

            return secret;
        }
        
        public Task<IPagedResults<TModel>> SelectAsync<TModel>(params object[] inputParams) where TModel : class
        {
            throw new NotImplementedException();
        }
        public Task<IPagedResults<UserSecret>> SelectAsync(params object[] inputParams)
        {
            // TODO
            throw new NotImplementedException();
        }
        
        #endregion

        #region "Private Methods"

        private async Task<int> InsertUpdateInternal(
            int id,
            int userId,
            string secret,
            int[] salts,
            string securityStamp)
        {
            string delimitedSalts = null;
            if (salts != null)
                delimitedSalts = salts.ToDelimitedString();

            var dbId = 0;
            using (var context = _dbContext)
            {
                dbId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "plato_sp_InsertUpdateUserSecret",
                    id,
                    userId,
                    secret.ToEmptyIfNull().TrimToSize(255),
                    delimitedSalts.ToEmptyIfNull().TrimToSize(255),
                    securityStamp.ToEmptyIfNull().TrimToSize(255));
            }

            return dbId;
        }



        #endregion
    }
}