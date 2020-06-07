

using System.Collections.Generic;

namespace Archetype
{
    public class CardData : ICardFactory
    {
        public int Cost { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public string RulesText { get; set; }
        public string ImagePath { get; set; }
        
        public IList<ActionParameterData> Actions { get; set; }

        public Card MakeCopy(Unit owner)
        {
            return new Card(owner, this);
        }
    }
}
