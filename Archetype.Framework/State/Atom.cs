using Archetype.Framework.Core;

namespace Archetype.Framework.State;


public interface IHasLabels
{
    [PathPart("label")]
    string? GetLabel(string labelKey);
    void SetLabel(string labelKey, string value);
    void RemoveLabel(string labelKey);
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
    IEnumerable<string>? GetFacet(string facetKey);
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

public interface IAtom : IHasStats, IHasFacets, IHasTags, IHasLabels, IValueWhence
{
    Guid Id { get; }
    [PathPart("zone")]
    IZone? Zone { get; set; }
}

internal abstract class Atom : IAtom
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public IZone? Zone { get; set; }
    
    protected Dictionary<string, int> _stats = new();
    protected Dictionary<string, string[]> _facets = new();
    protected Dictionary<string, string> _labels = new();
    protected HashSet<string> _tags = new();
    public int? GetStat(string statKey)
    {
        return _stats.TryGetValue(statKey, out var value) ? value : null;
    }

    public void SetStat(string statKey, int value)
    {
        _stats[statKey] = value;
    }

    public IEnumerable<string>? GetFacet(string facetKey)
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

    public string? GetLabel(string labelKey)
    {
        return _labels.TryGetValue(labelKey, out var value) ? value : null;
    }

    public void SetLabel(string labelKey, string value)
    { 
        _labels[labelKey] = value;
    }

    public void RemoveLabel(string labelKey)
    {
        _labels.Remove(labelKey);
    }
}