using System.Reflection;
using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cyclone.Common.SimpleEntity
{
    public static class GraphQlExtensions
    {
        /// <summary>
        /// Удобный метод: включает Relay Global Object Identification и автоматически
        /// регистрирует Node-типы (EntityNodeType T) для всех concrete типов,
        /// унаследованных от BaseEntity найденных в переданных сборках.
        /// </summary>
        /// <param name="builder">IRequestExecutorBuilder (AddGraphQLServer() returns this)</param>
        /// <param name="assemblies">
        /// Сборки для сканирования. Если null или пустой — будут сканированы загруженные AppDomain.CurrentDomain.GetAssemblies().
        /// </param>
        /// <param name="requireNodeAttribute">
        /// Если true (по-умолчанию) — регистрируются только типы, помеченные [Node].
        /// Если false — регистрируются все concrete-наследники BaseEntity.
        /// </param>
        public static IRequestExecutorBuilder AddEntityNode(
            this IRequestExecutorBuilder builder,
            Assembly[]? assemblies = null,
            bool requireNodeAttribute = true)
        {
            builder.AddGlobalObjectIdentification();

            var toScan = assemblies == null || assemblies.Length == 0
                ? AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))
                    .ToArray()
                : assemblies;

            var baseEntityType = typeof(BaseEntity);

            var entityTypes = toScan
                .SelectMany(GetTypesSafe)
                .Where(t => t is { IsClass: true, IsAbstract: false } && baseEntityType.IsAssignableFrom(t))
                .Distinct()
                .ToArray();

            if (requireNodeAttribute)
            {
                var nodeAttrType = typeof(NodeAttribute);
                entityTypes = entityTypes
                    .Where(t => Attribute.IsDefined(t, nodeAttrType))
                    .ToArray();
            }

            foreach (var runtimeType in entityTypes)
            {
                var generic = typeof(EntityNodeType<>).MakeGenericType(runtimeType);
                builder.AddType(generic);
            }

            return builder;
        }

        private static Type[] GetTypesSafe(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(t => t != null).ToArray()!;
            }
            catch
            {
                return [];
            }
        }
    }
}