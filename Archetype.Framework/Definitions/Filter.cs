using Archetype.Framework.Runtime;
using Archetype.Framework.Runtime.State;

namespace Archetype.Framework.Definitions;

public interface IAtomFilter
{
    public string FilterString { get; }

    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context);

    public bool FilterAtom(IAtom atom, IResolutionContext context);
}

public class Filter : IAtomFilter
{
    private record FilterToken(string Key, IReadOnlyList<string> MatchValues);
    private readonly List<FilterToken> _characteristics = new();
    private readonly List<string> _zoneSubtypes = new();
    
    private Filter(string filterString)
    {
        FilterString = filterString.Trim('(', ')', ' ');
        
        var filterTokens = FilterString.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var token in filterTokens)
        {
            var parts = token.Split(':', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"Invalid filter token: {token}");
            }

            var (key, values) = (parts[0], parts[1]);


            var matchValues = values.Split('|', StringSplitOptions.RemoveEmptyEntries);
            
            if (key == "zone")
            {
                _zoneSubtypes.AddRange(matchValues);
                continue;
            }
            
            _characteristics.Add(new FilterToken(key, matchValues));
        }
    }
    
    public static Filter Parse(string filter)
    {
        return new Filter(filter);
    }
    
    public string FilterString { get; set; }

    public IEnumerable<IAtom> ProvideAtoms(IResolutionContext context)
    {
        return context.GameState.Zones.Values
            .Where(z => _zoneSubtypes.Any(s => z.HasCharacteristic("SUBTYPE", s, context)))
            .SelectMany(z => z.Atoms)
            .Where(a => FilterAtom(a, context));
    }
    
    public bool FilterAtom(IAtom atom, IResolutionContext context)
    {
        return _characteristics.All(c => c.MatchValues.Any(mv => atom.HasCharacteristic(c.Key, mv, context)));
    }
}