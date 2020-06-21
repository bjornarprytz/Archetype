namespace Archetype
{
    public interface IPromptable
    {
        void Choose<T>(ISelectionInfo<T> selectionInfo);
    }
}
