namespace Archetype
{
    public class ActionModifiers : TypeDictionary<ActionInfo, int>
    {
        public void Add<T>(int modifier) where T : ActionInfo => Set<T>(Get<T>() + modifier);

        public int GetModifiedVal<T>(T action) where T : ActionInfo => action.Strength + Get<T>();
    }
}
