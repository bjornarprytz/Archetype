
using Newtonsoft.Json;
using Archetype;

namespace CardPort
{
    public static class CardSerializer
    {
        private static JsonSerializerSettings Settings => new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects };

        public static CardData DeserializeCardJson(string json)
        {
            return JsonConvert.DeserializeObject<CardData>(json, Settings);
        }

        public static string SerializeCardData(CardData cardData)
        {
            return JsonConvert.SerializeObject(cardData, Settings);
        }



    }
}
