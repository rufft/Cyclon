using Microsoft.EntityFrameworkCore;

namespace Cyclone.Common.SimpleEntity
{
    public sealed class EntityNodeType<T> : ObjectType<T>
        where T : BaseEntity
    {
        protected override void Configure(IObjectTypeDescriptor<T> descriptor)
        {
            descriptor
                .ImplementsNode()
                .IdField(e => e.Id)
                .ResolveNode(async (ctx, id) =>
                {
                    var db = ctx.Service<DbContext>();

                    var entity = await db.Set<T>().FindAsync([id], ctx.RequestAborted).ConfigureAwait(false);
                    return entity;
                });
        }
    }
}