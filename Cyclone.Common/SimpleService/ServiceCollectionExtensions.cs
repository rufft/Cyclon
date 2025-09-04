using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Cyclone.Common.SimpleService;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Добавляет все SimpleService T, TDbContext из указанной сборки в scoped сервисы
    /// </summary>
    /// <param name="services">Параметр расширения</param>
    /// <param name="assemblies">Сборки для сканирования. Если null, будет использована сборка вызывающего кода</param>
    /// <returns></returns>
    public static IServiceCollection AddSimpleServices(
        this IServiceCollection services,
        params Assembly[]? assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = [Assembly.GetCallingAssembly()];
        }

        foreach (var assembly in assemblies)
        {
            var serviceTypes = assembly.GetTypes()
                .Where(t => t is { IsAbstract: false, IsInterface: false })
                .Where(IsSimpleService);

            foreach (var type in serviceTypes)
            {
                services.AddScoped(type);
            }
        }

        return services;
    }

    private static bool IsSimpleService(Type type)
    {
        var baseType = type.BaseType;

        while (baseType != null)
        {
            if (baseType.IsGenericType && 
                baseType.GetGenericTypeDefinition() == typeof(SimpleService<,>))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }
        return false;
    }
}

