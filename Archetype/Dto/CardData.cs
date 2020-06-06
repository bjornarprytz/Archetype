
namespace Archetype
{
    public class CardData
    {
        public int Cost { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }

        public string RulesText { get; set; }
        public string ImagePath { get; set; }
        
        public ActionData[] Actions { get; set; }
    }
}
