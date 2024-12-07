using Archetype.Framework.Core;
using Archetype.Framework.Parsing;
using Archetype.Framework.Resolution;

namespace Archetype.Framework.State;

public interface IHasName
{
    [PathPart("name")]
    string GetName();
}

public interface IHasStats
{
    [PathPart("stats")]
    int? GetStat(string statKey);
    void SetStat(string statKey, int value);
}

public interface IHasFacets
{
    [PathPart("facets")]
    string[]? GetFacet(string facetKey);
    void SetFacet(string facetKey, string[] value);
    void RemoveFacet(string facetKey);
}

public interface IHasTags
{
    [PathPart("tags")]
    string[] GetTags();
    bool HasTag(string tag);
    void AddTag(string tag);
    void RemoveTag(string tag);
}

public interface IAtom : IHasStats, IHasFacets, IHasTags, IValueWhence
{
    Guid Id { get; }
    [PathPart("zone")]
    IZone? Zone { get; set; }
}

public abstract class Atom : IAtom
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public IZone? Zone { get; set; }
    
    protected Dictionary<string, int> _stats = new();
    protected Dictionary<string, string[]> _facets = new();
    protected HashSet<string> _tags = new();
    public int? GetStat(string statKey)
    {
        return _stats.TryGetValue(statKey, out var value) ? value : null;
    }

    public void SetStat(string statKey, int value)
    {
        _stats[statKey] = value;
    }

    public string[]? GetFacet(string facetKey)
    {
        return _facets.TryGetValue(facetKey, out var value) ? value : null;
    }

    public void SetFacet(string facetKey, string[] value)
    {
        _facets[facetKey] = value;
    }

    public void RemoveFacet(string facetKey)
    {
        _facets.Remove(facetKey);
    }

    public string[] GetTags()
    {
        return _tags.ToArray();
    }

    public bool HasTag(string tag)
    {
        return _tags.Contains(tag);
    }

    public void AddTag(string tag)
    {
        _tags.Add(tag);
    }

    public void RemoveTag(string tag)
    {
        _tags.Remove(tag);
    }
}