namespace Cyclone.Common.SimpleSoftDelete;

public static class DeletionTopics
{
    public static string For(string entityType) => $"{entityType}.Deleted";
    public static string For<T>() => For(typeof(T).Name);
}