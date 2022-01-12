using System.Collections.Generic;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IHistoryReader
    {
        IReadOnlyList<IHistoryEntry> Entries { get; }
        IReadOnlyDictionary<string, IList<IHistoryEntry>> CardEntriesByProtoGuid { get; }
        IReadOnlyDictionary<ICard, IList<IHistoryEntry>> CardEntriesByInstance { get; }
    }

    public interface IHistoryWriter
    {
        void Append(IContext context, IResultsReader result);
        void Append(IResultsReader result);
    }

    public interface IHistoryEntry
    {
        IResultsReader Result { get; }
    }
    
    internal class History : IHistoryReader, IHistoryWriter
    {
        private readonly List<IHistoryEntry> _entries = new();
        private readonly Dictionary<string, IList<IHistoryEntry>> _entriesByCardName = new();
        private readonly Dictionary<ICard, IList<IHistoryEntry>> _entriesByCardInstance = new();

        public IReadOnlyList<IHistoryEntry> Entries => _entries;
        public IReadOnlyDictionary<string, IList<IHistoryEntry>> CardEntriesByProtoGuid => _entriesByCardName;
        public IReadOnlyDictionary<ICard, IList<IHistoryEntry>> CardEntriesByInstance => _entriesByCardInstance;


        public void Append(IContext context, IResultsReader result)
        {
            var newEntry = new Entry(result);
            
            _entries.Add(newEntry);

            if (context is IContext<ICard> cardContext)
            {
                var card = cardContext.Source;
                
                (_entriesByCardName[card.Name] ??= new List<IHistoryEntry>()).Add(newEntry);
                (_entriesByCardInstance[card] ??= new List<IHistoryEntry>()).Add(newEntry);
            }
            
        }

        public void Append(IResultsReader result)
        {
            _entries.Add(new Entry(result));
        }
        
        private record Entry(IResultsReader Result)  : IHistoryEntry;
    }
}