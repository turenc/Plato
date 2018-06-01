﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Plato.Data.Abstractions;

namespace Plato.Data
{
    public class DbContextOptionsConfigure : IConfigureOptions<DbContextOptions>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DbContextOptionsConfigure()
        {
        }

        public DbContextOptionsConfigure(
            IServiceScopeFactory serivceScopeFactory)
        {
            _serviceScopeFactory = serivceScopeFactory;
        }

        public void Configure(DbContextOptions options)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {

                var configuration = scope.ServiceProvider.GetRequiredService<IConfigurationRoot>();

                // default configuration
                options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
                options.DatabaseProvider = "SqlProvider";
                options.TablePrefix = "";

            }

        }
    }
}