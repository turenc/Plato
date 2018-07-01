﻿using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Plato.Internal.Abstractions.SetUp;
using Plato.Entities.Handlers;
using Plato.Entities.Models;
using Plato.Entities.Repositories;
using Plato.Entities.Stores;
using Plato.Internal.Features.Abstractions;
using Plato.Internal.Hosting.Abstractions;
using Plato.Internal.Models.Shell;

namespace Plato.Entities
{
    public class Startup : StartupBase
    {
        private readonly IShellSettings _shellSettings;

        public Startup(IShellSettings shellSettings)
        {
            _shellSettings = shellSettings;
        }

        public override void ConfigureServices(IServiceCollection services)
        {

            // Set-up event handler
            services.AddScoped<ISetUpEventHandler, SetUpEventHandler>();

            // Feature event handler
            services.AddScoped<IFeatureEventHandler, FeatureEventHandler>();

            // Data Repositories
            services.AddScoped<IEntityRepository<Entity>, EntityRepository>();
            services.AddScoped<IEntityDataRepository<EntityData>, EntityDataRepository>();

            services.AddScoped<IEntityStore<Entity>, EntityStore>();
            services.AddScoped<IEntityDataStore<EntityData>, EntityDataStore>();

            //services.AddScoped<IEntityDetailsStore<EntityDetails>, EntityDetailsStore>();
            

        }

        public override void Configure(
            IApplicationBuilder app,
            IRouteBuilder routes,
            IServiceProvider serviceProvider)
        {
        }
    }
}