﻿using System.IO;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PlatoCore.Abstractions.Settings;
using PlatoCore.Security.Abstractions;
using PlatoCore.Security.Abstractions.Encryption;
using PlatoCore.Security.Attributes;
using PlatoCore.Security.Configuration;
using PlatoCore.Security.Encryption;

namespace PlatoCore.Security.Extensions
{
    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddPlatoSecurity(
            this IServiceCollection services)
        {
            services.AddPlatoAuthentication(); 
            services.AddPlatoAuthorization();
            services.AddPlatoEncryption();
            return services;
        }

        public static IServiceCollection AddPlatoAuthorization(this IServiceCollection services)
        {

            // Permissions manager
            services.AddScoped<IPermissionsManager<Permission>, PermissionsManager<Permission>>();

            // Add core authorization services 
            services.AddAuthorizationCore();

            return services;

        }

        public static IServiceCollection AddPlatoAuthentication(this IServiceCollection services)
        {            
            services.AddAuthentication();           
            return services;
        }

        public static IServiceCollection AddPlatoDataProtection(
            this IServiceCollection services)
        {

            // Attempt to get secrets path from appsettings.json file
            // If found register file system storage of private keys
            var opts = services.BuildServiceProvider().GetService<IOptions<PlatoOptions>>();
            if (opts != null)
            {
                if (!string.IsNullOrEmpty(opts.Value.SecretsPath))
                {
                    services.AddDataProtection()
                        .PersistKeysToFileSystem(new DirectoryInfo(opts.Value.SecretsPath));
                }
            }

            return services;

        }

        public static IServiceCollection AddPlatoModelValidation(
            this IServiceCollection services)
        {
            // Custom validation providers
            services.AddSingleton<IValidationAttributeAdapterProvider, CustomValidatiomAttributeAdapterProvider>();
            return services;
        }

        public static IServiceCollection AddPlatoEncryption(this IServiceCollection services)
        {

            // Key store
            services.AddSingleton<IEncrypterKeyStore, DefaultEncrypterKeyStore>();

            // Configuration
            services.AddSingleton<IConfigureOptions<PlatoKeyOptions>, PlatoKeyOptionsConfiguration>();

            // Default AES implementations
            services.AddSingleton<IEncrypterKeyBuilder, DefaultEncrypterKeyBuilder>();
            services.AddScoped<IEncrypter, DefaultEncrypter>();

            return services;

        }

    }

}
