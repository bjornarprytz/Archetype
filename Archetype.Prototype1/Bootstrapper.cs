using Archetype.Framework.Design;
using Archetype.Framework.Parsing;
using Archetype.Framework.State;
using Archetype.Prototype1.Proto;

namespace Archetype.Prototype1;

public class Bootstrapper(ISetParser setParser, IProtoData protoData, IRules rules) : IBootstrapper
{
    public void Bootstrap(string setJson)
    {
        protoData.AddSet(setParser.ParseSet(setJson));
        
        protoData.SetTurnSequence(
            new IPhase[]
            {
                new MainPhase(rules),
                new EnemyPhase(rules)
            });
    }
}