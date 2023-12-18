using Archetype.Framework.Design;
using Archetype.Framework.State;
using Archetype.Prototype1.Proto;

namespace Archetype.Prototype1;

public class Bootstrapper(ISetParser setParser, IProtoData protoData, IRules rules) : IBootstrapper
{
    public void Bootstrap(string setJson)
    {
        foreach (var set in setParser.ParseSets(setJson))
        {
            protoData.AddSet(set);
        }
        
        protoData.SetTurnSequence(
            new IPhase[]
            {
                new MainPhase(rules),
                new EnemyPhase(rules)
            });
    }
}