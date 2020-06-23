namespace Archetype
{
    public interface IPrompter
    {
        void Choose<T>(ISelectionInfo<T> selectionInfo);
    }
}
