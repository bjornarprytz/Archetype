namespace Archetype.Framework.Design;

public interface ISetParser
{
    public IEnumerable<IProtoSet> ParseSets(string setData);
}