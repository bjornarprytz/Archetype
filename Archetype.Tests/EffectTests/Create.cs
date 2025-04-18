using Archetype.Framework.State;

namespace Archetype.Tests;

public static class Create
{
    internal static Atom AtomWithFacets(string facetKey, params string[] facets)
    {
        var atom = new Atom()
        {
            State =
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
        var atom = Create.BasicAtom();
        
        if (value.HasValue)
        {
            atom.State.Stats[statKey] = value.Value;
        }
        
        return atom;
    }
    
    internal static Atom AtomWithTags(params string[] tags)
    {
        var atom = new Atom();
        
        foreach (var tag in tags)
        {
            atom.State.Tags.Add(tag);
        }
        
        return atom;
    }

    internal static Atom BasicAtom() => new Atom();
}