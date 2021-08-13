namespace Archetype.Core
{
    public interface ICardStack
    {
        void Push(ICard card, ICardArgs cardArgs);
    }
}