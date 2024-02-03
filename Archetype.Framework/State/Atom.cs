using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.State;

public interface IAtom
{
    Guid Id { get; }
    int? GetStat(string statKey);
    string? GetTag(string tagKey);
    T GetState<T>(string key);
    void SetState<T>(string key, T value);
    void RemoveState(string key);
    void ClearState();
    IZone? CurrentZone { get; set; }
}


public abstract class Atom : IAtom
{
    private readonly Dictionary<string, object> _state = new();
    public Guid Id { get; } = Guid.NewGuid();
    public int? GetStat(string statKey)
    {
        // TODO: Take state into account
        return Stats.TryGetValue(statKey, out var res) ? res : default;
    }

    public string? GetTag(string tagKey)
    {
        // TODO: Take state into account
        return Tags.TryGetValue(tagKey, out var res) ? res : default;
    }

    public T GetState<T>(string key)
    {
        if (!_state.TryGetValue(key, out var value)) return default;
        
        if (value is T typedValue)
            return typedValue;
            
        throw new InvalidOperationException($"State key {key} is not of type {typeof(T).Name}");
    }

    public void SetState<T>(string key, T value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));
        
        _state[key] = value;
    }
    
    public void RemoveState(string key)
    {
        _state.Remove(key);
    }
    
    public void ClearState()
    {
        _state.Clear();
    }

    public abstract IReadOnlyDictionary<string, int> Stats { get; }
    public abstract IReadOnlyDictionary<string, string> Tags { get; }
    
    public IZone? CurrentZone { get; set; }
}