using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Cyclone.Common.SimpleDatabase;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Регистрирует ваш контекст, наследованный от AppDbContext, и добавляет в DI список сборок,
    /// где искать сущности. `TContext` должен наследовать AppDbContext.
    /// </summary>
    public static IServiceCollection AddSimpleDbContext<TContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> optionsAction,
        Assembly[]? entityAssemblies = null,
        Action<ModelBuilder>? modelCustomization = null)
        where TContext : SimpleDbContext
    {
        services.AddDbContext<TContext>((serviceProvider, options) =>
        {
            optionsAction(options);
            // можно подключить логирование и пр. через serviceProvider
        });

        var assemblies = entityAssemblies ?? [];
        services.AddSingleton<IEnumerable<Assembly>>(assemblies);
        if (modelCustomization != null)
            services.AddSingleton(modelCustomization);

        return services;
    }
}