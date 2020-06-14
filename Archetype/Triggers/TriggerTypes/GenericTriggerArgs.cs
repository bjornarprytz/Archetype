namespace Archetype
{
    public class GenericTriggerArgs<T> : TriggerArgs
    {
        public T Value { get; set; }

        public GenericTriggerArgs(T value)
        {
            Value = value;
        }
    }
}
