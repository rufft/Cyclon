using HotChocolate.Execution;
using HotChocolate.Subscriptions;

namespace Cyclone.Common.SimpleSoftDelete;

public sealed class DeletionSubscription
{
    [Subscribe]
    public DeletionEvent OnAnyEntityDeleted([EventMessage] DeletionEvent ev) => ev;

    
    // Точечные подписки
    [Subscribe(With = nameof(SubscribeDisplayType))]
    public DeletionEvent OnDisplayTypeDeleted([EventMessage] DeletionEvent ev) => ev;
    
    [Subscribe(With = nameof(SubscribeDisplay))]
    public DeletionEvent OnDisplayDeleted([EventMessage] DeletionEvent ev) => ev;

    public ValueTask<ISourceStream<DeletionEvent>> SubscribeDisplayType(
        [Service] ITopicEventReceiver receiver, CancellationToken ct)
        => receiver.SubscribeAsync<DeletionEvent>(DeletionTopics.For("DisplayType"), ct);
    
    public ValueTask<ISourceStream<DeletionEvent>> SubscribeDisplay(
        [Service] ITopicEventReceiver receiver, CancellationToken ct)
        => receiver.SubscribeAsync<DeletionEvent>(DeletionTopics.For("Display"), ct);
}