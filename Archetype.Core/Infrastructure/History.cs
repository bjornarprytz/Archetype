using System.Reactive.Subjects;
using Archetype.Core.Atoms.Base;
using Archetype.Core.Extensions;
using Archetype.Core.Play;
using Archetype.View.Infrastructure;

namespace Archetype.Core.Infrastructure;

// TODO: Look at removing the ResultReader/Writer
public interface IResultsReader
{
    IEnumerable<IEffectResult> Results { get; }
}

public interface IResultsWriter
{
    void AddResult(IEffectResult effectEffectResult);
}

public interface IResultsReaderWriter : IResultsReader, IResultsWriter { }
    
    
    
    
public interface IHistoryReader
{
    IEnumerable<IHistoryEntry> Entries { get; }

    IEnumerable<IHistoryEntry> EntriesByName(string name);
    IEnumerable<IHistoryEntry> EntriesByAtom<T>(T atom) where T : IGameAtom;
}

public interface IHistoryWriter
{
    void Append(IContext context, IResultsReader result);
}

public interface IHistoryEntry
{
    IResultsReader Result { get; }
}
    
public interface IHistoryEmitter
{
    IObservable<IHistoryEntry> OnResult { get; }
}
    
internal class History : IHistoryReader, IHistoryWriter, IHistoryEmitter
{
    private readonly List<IHistoryEntry> _entries = new();
    private readonly Dictionary<string, List<IHistoryEntry>> _entriesByName = new();
    private readonly Dictionary<Guid, List<IHistoryEntry>> _entriesByAtom = new();
    private readonly Subject<IHistoryEntry> _onResult = new();

    public IObservable<IHistoryEntry> OnResult => _onResult;
    public IEnumerable<IHistoryEntry> Entries => _entries;
    public IEnumerable<IHistoryEntry> EntriesByName(string name)
    {
        return _entriesByName.ContainsKey(name)
            ? _entriesByName[name]
            : Enumerable.Empty<IHistoryEntry>();
    }

    IEnumerable<IHistoryEntry> IHistoryReader.EntriesByAtom<T>(T atom)
    {
        return _entriesByAtom.ContainsKey(atom.Guid)
            ? _entriesByAtom[atom.Guid]
            : Enumerable.Empty<IHistoryEntry>();
    }

    public void Append(IContext context, IResultsReader result)
    {
        var newEntry = new Entry(result);
            
        _entries.Add(newEntry);

        var atom = context.Source;
            
        _entriesByAtom.GetOrSet(atom.Guid).Add(newEntry);
            
        if (atom is IPiece piece)
        {
            _entriesByName.GetOrSet(piece.Name).Add(newEntry);
        }

        _onResult.OnNext(newEntry);
    }

    private record Entry(IResultsReader Result)  : IHistoryEntry;
}