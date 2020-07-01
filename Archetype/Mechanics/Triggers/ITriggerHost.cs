namespace Archetype
{
    public interface ITriggerHost
    {
        TypeDictionary<Trigger> Triggers { get; }

        void AttachTrigger<T>(T trigger) where T : Trigger;

        public void DetachTrigger<T>() where T : Trigger
        {
            if (Triggers.Has<T>())
            {
                Triggers.Get<T>().Detach();
                Triggers.Remove<T>();
            }
        }
    }
}
