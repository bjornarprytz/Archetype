using System.Collections.Generic;
using System.Linq;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Atoms;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IHistoryReader
    {
        IEnumerable<IHistoryEntry> Entries { get; }

        IEnumerable<IHistoryEntry> CardEntriesByName(string name);
        IEnumerable<IHistoryEntry> CardEntriesByInstance(ICard card);
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
        private readonly Dictionary<string, List<IHistoryEntry>> _entriesByCardName = new();
        private readonly Dictionary<ICard, List<IHistoryEntry>> _entriesByCardInstance = new();

        public IEnumerable<IHistoryEntry> Entries => _entries;
        public IEnumerable<IHistoryEntry> CardEntriesByName(string name)
        {
            return _entriesByCardName.ContainsKey(name)
                ? _entriesByCardName[name]
                : Enumerable.Empty<IHistoryEntry>();
        }

        IEnumerable<IHistoryEntry> IHistoryReader.CardEntriesByInstance(ICard card)
        {
            return _entriesByCardInstance.ContainsKey(card)
                ? _entriesByCardInstance[card]
                : Enumerable.Empty<IHistoryEntry>();
        }

        public void Append(IContext context, IResultsReader result)
        {
            var newEntry = new Entry(result);
            
            _entries.Add(newEntry);

            if (context is IContext<ICard> cardContext)
            {
                var card = cardContext.Source;
                
                _entriesByCardName.GetOrSet(card.Name).Add(newEntry);
                _entriesByCardInstance.GetOrSet(card).Add(newEntry);
            }
            
        }

        public void Append(IResultsReader result)
        {
            _entries.Add(new Entry(result));
        }
        
        private record Entry(IResultsReader Result)  : IHistoryEntry;
    }
}