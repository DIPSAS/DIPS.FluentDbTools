﻿using FluentDbTools.Extensions.MSDependencyInjection;
using FluentDbTools.Migration.Abstractions;
using FluentMigrator;
using FluentMigrator.Runner.VersionTableInfo;
using Microsoft.Extensions.DependencyInjection;

namespace FluentDbTools.Migration.Common
{
    public static class ServiceRegistration
    {
        public static IServiceCollection Register(IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddScoped<IFluentDbToolsVersionTableMetaData, VersionTableMetaData>()
                .AddScoped<IVersionTableMetaData>(sp => sp.GetRequiredService<IFluentDbToolsVersionTableMetaData>());
        }
    }
}
