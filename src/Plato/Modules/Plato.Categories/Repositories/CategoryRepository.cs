﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Plato.Categories.Models;
using Plato.Internal.Abstractions;
using Plato.Internal.Abstractions.Extensions;
using Plato.Internal.Data.Abstractions;

namespace Plato.Categories.Repositories
{

    public class CategoryRepository<TCategory> : ICategoryRepository<TCategory> where TCategory : class, ICategory
    {

        #region "Constructor"

        private readonly ICategoryDataRepository<CategoryData> _categoryDataRepository;
        private readonly IDbContext _dbContext;
        private readonly ILogger<CategoryRepository<TCategory>> _logger;

        public CategoryRepository(
            IDbContext dbContext,
            ILogger<CategoryRepository<TCategory>> logger,
            ICategoryDataRepository<CategoryData> categoryDataRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _categoryDataRepository = categoryDataRepository;
        }

        #endregion

        #region "Implementation"

        public async Task<TCategory> InsertUpdateAsync(TCategory model)
        {

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var id = await InsertUpdateInternal(
                model.Id,
                model.ParentId,
                model.FeatureId,
                model.Name,
                model.Description,
                model.Alias,
                model.IconCss,
                model.ForeColor,
                model.BackColor,
                model.SortOrder,
                model.CreatedUserId,
                model.CreatedDate,
                model.ModifiedUserId,
                model.ModifiedDate,
                model.Data);
            if (id > 0)
            {
                return await SelectByIdAsync(id);
            }

            return null;
        }

        public async Task<TCategory> SelectByIdAsync(int id)
        {

            TCategory category = null;
            using (var context = _dbContext)
            {
                category = await context.ExecuteReaderAsync(
                    CommandType.StoredProcedure,
                    "SelectCategoryById",
                    async reader => await BuildCategoryFromResultSets(reader),
                    id);
                
            }

            return category;

        }

        public async Task<IPagedResults<TCategory>> SelectAsync(params object[] inputParams)
        {
            IPagedResults<TCategory> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<IPagedResults<TCategory>>(
                    CommandType.StoredProcedure,
                    "SelectCategoriesPaged",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new PagedResults<TCategory>();
                            while (await reader.ReadAsync())
                            {
                                var category = ActivateInstanceOf<TCategory>.Instance();
                                category.PopulateModel(reader);
                                output.Data.Add(category);
                            }

                            if (await reader.NextResultAsync())
                            {
                                if (reader.HasRows)
                                {
                                    await reader.ReadAsync();
                                    output.PopulateTotal(reader);
                                }
                            }

                        }

                        return output;
                    },
                    inputParams);

            }

            return output;
        }

        public async Task<bool> DeleteAsync(int id)
        {

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation($"Deleting category with id: {id}");
            }

            var success = 0;
            using (var context = _dbContext)
            {
                success = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "DeleteCategoryById", id);
            }

            return success > 0 ? true : false;

        }

        public async Task<IEnumerable<TCategory>> SelectByFeatureIdAsync(int featureId)
        {

            IList<TCategory> output = null;
            using (var context = _dbContext)
            {
                output = await context.ExecuteReaderAsync<IList<TCategory>>(
                    CommandType.StoredProcedure,
                    "SelectCategoriesByFeatureId",
                    async reader =>
                    {
                        if ((reader != null) && (reader.HasRows))
                        {
                            output = new List<TCategory>();
                            while (await reader.ReadAsync())
                            {
                                var category = ActivateInstanceOf<TCategory>.Instance();
                                category.PopulateModel(reader);
                                output.Add(category);
                            }
                        }

                        return output;

                    },
                    featureId);
               
            }

            return output;

        }

        #endregion

        #region "Private Methods"

        async Task<TCategory> BuildCategoryFromResultSets(DbDataReader reader)
        {

            TCategory model = null;
            if ((reader != null) && (reader.HasRows))
            {

                // Category
                model = ActivateInstanceOf<TCategory>.Instance();

                await reader.ReadAsync();
                model.PopulateModel(reader);
               
                // Data
                if (await reader.NextResultAsync())
                {
                    if (reader.HasRows)
                    {
                        var data = new List<CategoryData>();
                        while (await reader.ReadAsync())
                        {
                            data.Add(new CategoryData(reader));
                        }
                        model.Data = data;
                    }

                }

            }

            return model;
        }


        async Task<int> InsertUpdateInternal(
            int id,
            int parentId,
            int featureId,
            string name,
            string description,
            string alias,
            string iconCss,
            string foreColor,
            string backColor,
            int sortOrder,
            int createdUserId,
            DateTimeOffset? createdDate,
            int modifiedUserId,
            DateTimeOffset? modifiedDate,
            IEnumerable<CategoryData> data)
        {

            var categoryId = 0;
            using (var context = _dbContext)
            {
                categoryId = await context.ExecuteScalarAsync<int>(
                    CommandType.StoredProcedure,
                    "InsertUpdateCategory",
                    id,
                    parentId,
                    featureId,
                    name.ToEmptyIfNull().TrimToSize(255),
                    description.ToEmptyIfNull().TrimToSize(500),
                    alias.ToEmptyIfNull().TrimToSize(255),
                    iconCss.ToEmptyIfNull().TrimToSize(50),
                    foreColor.ToEmptyIfNull().TrimToSize(50),
                    backColor.ToEmptyIfNull().TrimToSize(50),
                    sortOrder,
                    createdUserId,
                    createdDate.ToDateIfNull(),
                    modifiedUserId,
                    modifiedDate.ToDateIfNull(),
                    new DbDataParameter(DbType.Int32, ParameterDirection.Output));
            }

            // Add category data
            if (categoryId > 0)
            {
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        item.CategoryId = categoryId;
                        await _categoryDataRepository.InsertUpdateAsync(item);
                    }
                }

            }

            return categoryId;

        }

        #endregion

    }
    
}