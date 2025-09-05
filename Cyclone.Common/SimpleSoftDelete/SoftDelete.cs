using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Cyclone.Common.SimpleSoftDelete;

public class NavigationPolicy
{
    public string NavigationName { get; }
    public Type ChildType { get; }
    public bool IsCollection { get; }
    public Func<object, bool>? Predicate { get; } // compiled predicate, accepts object (child)

    public NavigationPolicy(string navigationName, Type childType, bool isCollection, Func<object, bool>? predicate)
    {
        NavigationName = navigationName;
        ChildType = childType;
        IsCollection = isCollection;
        Predicate = predicate;
    }
}

public static class SoftDeletePolicyRegistry
{
    private static readonly ConcurrentDictionary<Type, List<NavigationPolicy>> Policies = new();

    public static void RegisterCollection<TParent, TChild>(
        Expression<Func<TParent, IEnumerable<TChild>>> navigationSelector,
        Expression<Func<TChild, bool>>? childPredicate = null)
    {
        var name = GetMemberName(navigationSelector);
        var pred = childPredicate?.Compile();
        Func<object, bool>? wrap = pred == null ? null : obj => pred((TChild)obj);
        var policy = new NavigationPolicy(name, typeof(TChild), isCollection: true, predicate: wrap);
        var list = Policies.GetOrAdd(typeof(TParent), _ => []);
        list.Add(policy);
    }

    public static void RegisterReference<TParent, TChild>(
        Expression<Func<TParent, TChild?>> navigationSelector,
        Expression<Func<TChild, bool>>? childPredicate = null)
        where TChild : class
    {
        var name = GetMemberName(navigationSelector);
        var pred = childPredicate?.Compile();
        Func<object, bool>? wrap = pred == null ? null : obj => pred((TChild)obj);
        var policy = new NavigationPolicy(name, typeof(TChild), isCollection: false, predicate: wrap);
        var list = Policies.GetOrAdd(typeof(TParent), _ => []);
        list.Add(policy);
    }

    public static IReadOnlyList<NavigationPolicy> GetPoliciesFor(Type parentType) =>
        Policies.TryGetValue(parentType, out var list) ? list : Array.Empty<NavigationPolicy>();

    private static string GetMemberName(LambdaExpression lambda)
    {
        return lambda.Body switch
        {
            // ожидаем MemberExpression: p => p.SomeNav
            MemberExpression me => me.Member.Name,
                
            // возможно: UnaryExpression (например при конверсиях)
            UnaryExpression { Operand: MemberExpression me2 } => me2.Member.Name,
            _ => throw new ArgumentException(
                "Нельзя получить имя навигации из выражения. Используйте простой селектор: x => x.NavProperty")
        };
    }
}