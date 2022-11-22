using Archetype.Core.Proto;

namespace Archetype.Core.Infrastructure;

public interface IProtoFinder
{
    public T FindProto<T>(string name) where T : IProtoData;
}