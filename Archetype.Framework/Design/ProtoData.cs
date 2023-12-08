using Archetype.Framework.State;

namespace Archetype.Framework.Design;

public interface IProtoData
{
    IProtoCard? GetProtoCard(string name);
    IReadOnlyList<IPhase>? TurnSequence { get; }
    IEnumerable<IProtoSet> Sets { get; }
    
    void AddSet(IProtoSet set);
    void SetTurnSequence(IReadOnlyList<IPhase> turnSequence);
}

public class ProtoData : IProtoData
{
    private readonly Dictionary<string, IProtoSet> _sets = new();
    private readonly Dictionary<string, IProtoCard> _cards = new();
    
    public IProtoCard? GetProtoCard(string name)
    {
        return _cards.TryGetValue(name, out var card) ? card : null;
    }

    public IReadOnlyList<IPhase>? TurnSequence { get; private set; } = null;
    public IEnumerable<IProtoSet> Sets => _sets.Values;

    public void AddSet(IProtoSet set)
    {
        if (_sets.ContainsKey(set.Name))
            throw new InvalidOperationException($"Set with name {set.Name} already exists");
        
        _sets.Add(set.Name, set);
        
        foreach (var card in set.Cards)
        {
            if (_cards.ContainsKey(card.Name))
                throw new InvalidOperationException($"Card with name {card.Name} already exists");
            
            _cards.Add(card.Name, card);
        }
    }

    public void SetTurnSequence(IReadOnlyList<IPhase> turnSequence)
    {
        if (TurnSequence != null)
            throw new InvalidOperationException("Turn sequence already set");
        
        TurnSequence = turnSequence;
    }
}

