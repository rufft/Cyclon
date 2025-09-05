namespace Cyclone.Common.SimpleSoftDelete.Abstractions;

public interface IDeletionSubscriptionRegistry
{
    void SubscribeTopic(string topic, DeletionEventHandler handler);
    void Subscribe(string subscriptionName, DeletionEventHandler handler);
    IReadOnlyDictionary<string, List<DeletionEventHandler>> GetAll();
}