using Archetype.Framework.Design;
using Archetype.Framework.State;

namespace Archetype.Prototype1;

public class Bootstrapper(ISetParser setParser, IProtoData protoData, IRules rules) : IBootstrapper
{
    public void Bootstrap(string setJson)
    {
        foreach (var set in setParser.ParseSets(setJson))
        {
            protoData.AddSet(set);
        }
    }
}