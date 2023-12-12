using Archetype.Framework.Design;
using Archetype.Framework.Parsing;
using Archetype.Framework.State;
using Archetype.Prototype1.Proto;

namespace Archetype.Prototype1;

public class Bootstrapper(string setJson) : IBootstrapper
{
    
    public void Bootstrap(IProtoData protoData, IRules rules)
    {
        var setParser = null as ISetParser;// new SetParser(new CardParser(rules));
        protoData.AddSet(setParser.ParseSet(setJson));
        
        protoData.SetTurnSequence(
            new IPhase[]
            {
                new MainPhase(rules),
                new EnemyPhase(rules)
            });
    }
}