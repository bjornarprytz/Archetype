using Archetype.Framework.DependencyInjection;
using Archetype.Framework.Design;
using Archetype.Framework.Parsing;
using Archetype.Framework.State;
using Archetype.Prototype1.Proto;

namespace Archetype.Prototype1;

public class Bootstrapper(IProtoData protoData, IRules rules, IPhaseFactory phaseFactory, ISetParser setParser)
{
    public void Bootstrap(string setJson)
    {
        AddCards(setJson);
        AddTurnSequence();
    }
    
    private void AddCards(string setJson)
    {
        protoData.AddSet(setParser.ParseSet(setJson));
    }
    
    private void AddTurnSequence()
    {
        protoData.SetTurnSequence(
            new IPhase[]
            {
                phaseFactory.CreatePhase<MainPhase>(),
                phaseFactory.CreatePhase<EnemyPhase>()
            });
    }
}