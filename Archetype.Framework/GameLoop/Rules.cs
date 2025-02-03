using System.Reflection;
using Archetype.Framework.Core;
using Archetype.Framework.Events;
using Archetype.Framework.Resolution;

namespace Archetype.Framework.GameLoop;

public interface IRules
{
    IEnumerable<IEvent> ResolveEffect(IResolutionContext context, EffectProto effectProto);
    
    IEnumerable<ICardProto> GetCardPool();
    ICardProto? GetCard(string cardName);
}

public interface IKeyword
{
    public string Keyword { get; }
}
