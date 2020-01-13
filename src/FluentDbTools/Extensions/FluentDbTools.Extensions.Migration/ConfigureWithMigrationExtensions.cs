﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FluentDbTools.Common.Abstractions;
using FluentDbTools.Extensions.MSDependencyInjection;
using FluentDbTools.Extensions.Migration.DefaultConfigs;
using FluentDbTools.Migration.Abstractions;
using FluentDbTools.Migration.Oracle;
using FluentDbTools.Migration.Postgres;
using FluentMigrator;
using FluentMigrator.Runner.Generators.Oracle;
using FluentMigrator.Runner.Generators.Postgres;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FluentDbTools.Extensions.Migration
{
    public static class ConfigureWithMigrationExtensions
    {
        public static IServiceCollection ConfigureWithMigrationAndScanForVersionTable(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assembliesWithMigrationModels)
        {
            var assembliesWithMigrationModelArray = assembliesWithMigrationModels.ToArray();

            return serviceCollection
                .ConfigureWithMigration(assembliesWithMigrationModelArray)
                .ConfigureVersionTableMetaData(assembliesWithMigrationModelArray);
        }

        public static IServiceCollection ConfigureWithMigration(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assembliesWithMigrationModels)
        {
            var assembliesWithMigrationModelArray = assembliesWithMigrationModels.ToArray();
            return serviceCollection
                .AddDefaultDbMigrationConfig()
                .Register(FluentDbTools.Migration.Oracle.ServiceRegistration.Register)
                .Register(FluentDbTools.Migration.Postgres.ServiceRegistration.Register)
                .RegisterDatabaseDependedFluentMigrationTypes()
                .ConfigureWithMigrationAssemblies(FluentDbTools.Migration.ServiceRegistration.Register, assembliesWithMigrationModelArray);
        }

        internal static IServiceCollection ConfigureVersionTableMetaData(
            this IServiceCollection serviceCollection,
            IEnumerable<Assembly> assembliesWithMigrationModels)
        {
            var serviceType = typeof(IVersionTableMetaData);
            foreach (var assembliesWithMigrationModel in assembliesWithMigrationModels)
            {
                var implementationType = assembliesWithMigrationModel
                    .GetTypes()
                    .FirstOrDefault(x => !x.IsInterface && !x.IsAbstract && serviceType.IsAssignableFrom(x));

                if (implementationType == null)
                {
                    continue;
                }

                serviceCollection.AddScoped(serviceType, implementationType);
                return serviceCollection;
            }

            return serviceCollection;
        }

        public static IServiceCollection AddDefaultDbMigrationConfig(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddDefaultDbConfig();
            serviceCollection.TryAddScoped<IDbMigrationConfig, MsDbMigrationConfig>();
            return serviceCollection;
        }

        public static IServiceCollection RegisterDatabaseDependedFluentMigrationTypes(this IServiceCollection serviceCollection)
        {
            return serviceCollection
            .AddScoped<IMigrationProcessor>(sp =>
            {
                var isPostgres = sp.GetService<IDbMigrationConfig>()?.DbType == SupportedDatabaseTypes.Postgres;
                if (isPostgres)
                {
                    return sp.GetRequiredService<ExtendedPostgresProcessor>();
                }

                return sp.GetRequiredService<ExtendedOracleManagedProcessor>();
            })
            .AddScoped<IMigrationGenerator>(sp =>
            {
                var isPostgres = sp.GetService<IDbMigrationConfig>()?.DbType == SupportedDatabaseTypes.Postgres;
                if (isPostgres)
                {
                    return sp.GetRequiredService<PostgresGenerator>();
                }

                return sp.GetRequiredService<OracleGenerator>();
            });


        }

    }
}