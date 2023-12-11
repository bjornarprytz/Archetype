namespace Archetype.Framework.Design;

public interface IBootstrapper
{
    void Bootstrap(IProtoData protoData, IRules rules);
}