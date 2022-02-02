using Archetype.Core.Infrastructure;
using Archetype.Core.Play;
using Archetype.View.Atoms;
using HotChocolate.Types;

namespace Archetype.Server.Schema;

public class HistoryEntryType : ObjectType<IHistoryEntry>
{
    protected override void Configure(IObjectTypeDescriptor<IHistoryEntry> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("A historic event");

        descriptor.Ignore(h => h.Result);
        descriptor.Field(h => h.Result.Results)
            .Type<ListType<ResultUnion>>();
    }
    
}


public class IntResultType : ResultType<IEffectResult<int>>
{
    protected override void Configure(IObjectTypeDescriptor<IEffectResult<int>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(r => r.Result)
            .Name("intResult")
            ;
    }
}

public class AtomResultType : ResultType<IEffectResult<IGameAtomFront>>
{
    protected override void Configure(IObjectTypeDescriptor<IEffectResult<IGameAtomFront>> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Field(r => r.Result)
            .Type<AtomUnion>()
            .Name("atomResult")
            ;
    }
}

public class AggregateResultType : ResultType<IAggregateEffectResult>
{
    protected override void Configure(IObjectTypeDescriptor<IAggregateEffectResult> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Ignore(r => r.Result);
        descriptor.Ignore(r => r.Verb);
        descriptor.Ignore(r => r.Affected);
    }
}

public abstract class ResultType<T> : ObjectType<T>
    where T : IEffectResult
{
    protected override void Configure(IObjectTypeDescriptor<T> descriptor)
    {
        base.Configure(descriptor);

        descriptor.Description("Results of an event");

        descriptor.IsOfType((context, result) => result is T);
        
        descriptor.Field(r => r.IsNull);
        descriptor.Field(r => r.Verb);
        descriptor.Field(r => r.Affected)
            .Type<AtomUnion>();
        
        descriptor.Field(r => r.SideEffects)
            .Type<ListType<ResultUnion>>();
    }
}

public class ResultUnion : UnionType
{
    protected override void Configure(IUnionTypeDescriptor descriptor)
    {
        base.Configure(descriptor);

        descriptor.Type<IntResultType>();
        descriptor.Type<AtomResultType>();
        descriptor.Type<AggregateResultType>();
    }
}