namespace Archetype
{
    public interface IPromptable
    {
        void Choose<T>(Choose<T> chooseArgs) where T : GamePiece;
    }
}
