using Archetype.Framework.State;

namespace Archetype.Tests;

public static class Create
{
    internal static Atom AtomWithFacets(string facetKey, string[] facets)
    {
        var atom = new Atom()
        {
            Type = "Some Atom",
            BaseState =
            {
                Facets =
                {
                    [facetKey] = facets
                }
            }
        };
        
        return atom;
    }
    
    internal static Atom AtomWithStats(string statKey, int? value)
    {
        var atom = new Atom()
        {
            Type = "Some Atom",
        };
        
        if (value.HasValue)
        {
            atom.BaseState.Stats[statKey] = value.Value;
        }
        
        return atom;
    }
    
    internal static Atom AtomWithTags(params string[] tags)
    {
        var atom = new Atom()
        {
            Type = "Some Atom"
        };
        
        foreach (var tag in tags)
        {
            atom.BaseState.Tags.Add(tag);
        }
        
        return atom;
    }

    internal static Atom BasicAtom() => new Atom()
    {
        Type = "Some Atom",
    };
}