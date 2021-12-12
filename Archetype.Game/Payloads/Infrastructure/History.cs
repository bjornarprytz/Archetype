using System;
using System.Collections.Generic;
using Archetype.Game.Payloads.Context;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Pieces;
using Archetype.Game.Payloads.Pieces.Base;

namespace Archetype.Game.Payloads.Infrastructure
{
    public interface IHistoryReader
    {
        IReadOnlyList<IHistoryEntry> Entries { get; }
        IReadOnlyDictionary<string, IList<ICardEntry>> CardEntriesByProtoGuid { get; }
        IReadOnlyDictionary<ICard, IList<ICardEntry>> CardEntriesByInstance { get; }
    }

    public interface IHistoryWriter
    {
        void Append(ICardContext context, IResolution result); // TODO: Simplify here: Use a generic context istead
        void Append(IResolution result);
    }

    public interface IHistoryEntry
    {
        IResolution Result { get; }
    }

    public interface ICardEntry : IHistoryEntry
    {
        ICardContext Context { get; }
    }
    
    public class History : IHistoryReader, IHistoryWriter
    {
        private readonly List<IHistoryEntry> _entries = new();
        private readonly Dictionary<string, IList<ICardEntry>> _entriesByProtoGuid = new();
        private readonly Dictionary<ICard, IList<ICardEntry>> _entriesByCardInstance = new();

        public IReadOnlyList<IHistoryEntry> Entries => _entries;
        public IReadOnlyDictionary<string, IList<ICardEntry>> CardEntriesByProtoGuid => _entriesByProtoGuid;
        public IReadOnlyDictionary<ICard, IList<ICardEntry>> CardEntriesByInstance => _entriesByCardInstance;


        public void Append(ICardContext context, IResolution result)
        {
            var newEntry = new CardEntry(context, result);
            
            _entries.Add(newEntry);
            
            var card = context.PlayArgs.Card;
            
            (_entriesByProtoGuid[card.Name] ??= new List<ICardEntry>()).Add(newEntry);
            (_entriesByCardInstance[card] ??= new List<ICardEntry>()).Add(newEntry);
        }

        public void Append(IResolution result)
        {
            _entries.Add(new Entry(result));
        }

        private record CardEntry(ICardContext Context, IResolution Result) : ICardEntry;
        private record Entry(IResolution Result)  : IHistoryEntry;
    }
}