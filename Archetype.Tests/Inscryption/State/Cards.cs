using Archetype.Framework.Parsing;
using Archetype.Framework.Runtime.State;
using Archetype.Tests.Rules.Inscryption;

namespace Archetype.Tests.Inscryption.Cards;

public static class Setup
{
    public static IEnumerable<ICard> Cards()
    {
        var cardsDatas = new List<CardData>
        {
            new()
            {
                Text = 
"""
(NAME "Wolf")
(STATIC 
    (TRIBE BEAST)
    (POWER 3)
    (HEALTH 2)
)
(EFFECTS 
    {(COSTS (BLOOD_COST 2))}
)
"""
            }
        };

        var rules = Build.InscryptionRules();

        var cardParser = new CardParser(rules);
        
        foreach (var cardData in cardsDatas)
        {
            yield return new Card(cardParser.ParseCard(cardData));
        }
    }
}