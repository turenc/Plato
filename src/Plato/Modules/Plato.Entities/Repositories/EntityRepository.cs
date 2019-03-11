﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Entities.Models;
using Plato.Internal.Abstractions;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;

namespace Plato.Entities.Repositories
{

    public class EntityRepository<TModel> : IEntityRepository<TModel> where TModel : class, IEntity
    {
     
        #region "Constructor"
        
        private readonly IDbContext _dbContext;
        private readonly ILogger<EntityRepository<TModel>> _logger;
        private readonly IEntityDataRepository<IEntityData> _entityDataRepository;

        public EntityRepository(
            IDbContext dbContext,
            ILogger<EntityRepository<TModel>> logger,
            IEntityDataRepository<IEntityData> entityDataRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _entityDataRepository = entityDataRepository;
        }

        #endregion
        
        #region "Implementation"

        public async Task<TModel> InsertUpdateAsync(TModel entity)
        {

            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
                
            var id = await InsertUpdateInternal(
                entity.Id,
                entity.ParentId,
                entity.FeatureId,
                entity.CategoryId,
                entity.Title,
                entity.Alias,
                entity.Message,
                entity.Html,
                entity.Abstract,
                entity.Urls,
                entity.IsPrivate,
                entity.IsSpam,
                entity.IsPinned,
                entity.IsDeleted,
                entity.IsClosed,
                entity.TotalViews,
                entity.TotalReplies,
                entity.TotalParticipants,
                entity.TotalReactions,
                entity.TotalFollows,
                entity.TotalReports,
                entity.TotalStars,
                entity.DailyViews,
                entity.DailyReplies,
                entity.DailyReactions,
                entity.DailyFollows,
                entity.DailyReports,
                entity.DailyStars,
                entity.CreatedUserId,
                entity.CreatedDate,
                entity.EditedUserId,
                entity.EditedDate,
                entity.ModifiedUserId,
                entity.ModifiedDate,
                entity.LastReplyId,
                entity.LastReplyUserId,
                entity.LastReplyDate,
                entity.Data);
            if (id > 0)
            {
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<TModel> SelectByIdAsync(int id)
        {

            TModel entity = null;
            using (var context = _dbContext)
            {
                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectEntityById", id);
                entity = await BuildEntityFromResultSets(reader);
            }

            return entity;

        }
        
        public async Task<IPagedResults<TModel>> SelectAsync(params object[] inputParams)
        {
            PagedResults<TModel> output = null;
            using (var context = _dbContext)
            {

                var reader = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectEntitiesPaged",
                    inputParams);
                if ((reader != null) && (reader.HasRows))
                {
                    output = new PagedResults<TModel>();
                    while (await reader.ReadAsync())
                    {
                        var entity = ActivateInstanceOf<TModel>.Instance();
                        entity.PopulateModel(reader);
                        output.Data.Add(entity);
                    }

                    if (await reader.NextResultAsync())
                    {
                        await reader.ReadAsync();
                        output.PopulateTotal(reader);
                    }

                }
            }

            return output;
        }


        public async Task<bool> DeleteAsync(int id)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting entity with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteEntityById", id);
            }

            return success > 0 ? true : false;

        }

        #endregion

        #region "Private Methods"

        async Task<TModel> BuildEntityFromResultSets(DbDataReader reader)
        {

            TModel model = null;
            if ((reader != null) && (reader.HasRows))
            {
                model = ActivateInstanceOf<TModel>.Instance();
               
                // Entity
                await reader.ReadAsync();
                model.PopulateModel(reader);

                // Data
                if (await reader.NextResultAsync())
                {
                    var data = new List<EntityData>();
                    while (await reader.ReadAsync())
                    {
                        var entityData = new EntityData(reader);
                        data.Add(entityData);
                    }

                    model.Data = data;
                }

            }

            return model;
        }

        async Task<int> InsertUpdateInternal(
            int id,
            int parentId,
            int featureId,
            int categoryId,
            string title,
            string alias,
            string message,
            string html,
            string messageAbstract,
            string urls,
            bool isPrivate,
            bool isSpam,
            bool isPinned,
            bool isDeleted,
            bool isClosed,
            int totalViews,
            int totalReplies,
            int totalParticipants,
            int totalReactions,
            int totalFollows,
            int totalReports,
            int totalStars,
            double meanViews,
            double meanReplies,
            double meanReactions,
            double meanFollows,
            double meanReports,
            double meanStars,
            int createdUserId,
            DateTimeOffset? createdDate,
            int editedUserId,
            DateTimeOffset? editedDate,
            int modifiedUserId,
            DateTimeOffset? modifiedDate,
            int lastReplyId,
            int lastReplyUserId,
            DateTimeOffset? lastReplyDate,
            IEnumerable<IEntityData> data)
        {

            var entityId = 0;
            using (var context = _dbContext)
            {
                entityId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateEntity",
                    id,
                    parentId,
                    featureId,
                    categoryId,
                    title.ToEmptyIfNull().TrimToSize(255),
                    alias.ToEmptyIfNull().TrimToSize(255),
                    message.ToEmptyIfNull(),
                    html.ToEmptyIfNull(),
                    messageAbstract.ToEmptyIfNull().TrimToSize(500),
                    urls.ToEmptyIfNull(),
                    isPrivate,
                    isSpam,
                    isPinned,
                    isDeleted,
                    isClosed,
                    totalViews,
                    totalReplies,
                    totalParticipants,
                    totalReactions,
                    totalFollows,
                    totalReports,
                    totalStars,
                    meanViews,
                    meanReplies,
                    meanReactions,
                    meanFollows,
                    meanReports,
                    meanStars,
                    createdUserId,
                    createdDate.ToDateIfNull(),
                    editedUserId,
                    editedDate,
                    modifiedUserId,
                    modifiedDate,
                    lastReplyId,
                    lastReplyUserId,
                    lastReplyDate.ToDateIfNull(),
                    new DbDataParameter(DbType.Int32, ParameterDirection.Output));
            }
            
            // Add entity data
            if (entityId > 0)
            {
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.EntityId = entityId;
                        item.ModifiedDate = DateTimeOffset.UtcNow;
                        await _entityDataRepository.InsertUpdateAsync(item);
                    }
                }

            }

            return entityId;

        }
        
        #endregion
    }

}