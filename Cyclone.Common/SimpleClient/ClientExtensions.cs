using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cyclone.Common.SimpleClient;

public static class ClientExtensions
{
    public static IServiceCollection AddSimpleGraphQlClient(
        this IServiceCollection service)
    {
        service.AddScoped<SimpleClient>();
        return service;
    }
}