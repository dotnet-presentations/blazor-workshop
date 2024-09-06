// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using BlazingPizza.ComponentsLibrary;

namespace Microsoft.Extensions.DependencyInjection;

public static class LocalStorageServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="LocalStorageJSInterop"/> as a scoped-lifetime service
    /// to the given <paramref name="services"/> collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The same service instance given, with the added service.</returns>
    public static IServiceCollection AddLocalStorageJSInterop(this IServiceCollection services)
    {
        services.AddScoped<LocalStorageJSInterop>();

        return services;
    }
}
