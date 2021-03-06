﻿using Microsoft.Extensions.DependencyInjection;
using PlatoCore.Data.Migrations.Extensions;
using PlatoCore.Data.Schemas.Extensions;
using PlatoCore.Data.Abstractions;
using PlatoCore.Data.Providers;

namespace PlatoCore.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        
        public static IServiceCollection AddPlatoDbContext(
            this IServiceCollection services)
        {

            // Add default data options and data context
            // DbContextOptions is overriden for each tennet within ShellContainerFactory
            services.AddScoped<IDbContextOptions, DbContextOptions>();

            services.AddScoped<IDataProvider, SqlProvider>();
            services.AddScoped<IDbContext, DbContext>();

            //services.AddSingleton<IConfigureOptions<DbContextOptions>, DbContextOptionsConfigure>();
            services.AddTransient<IDbHelper, DbHelper>();

            // Add schemas
            services.AddDataSchemas();

            // Add migrations 
            services.AddDataMigrations();

            return services;

        }


    }
}
