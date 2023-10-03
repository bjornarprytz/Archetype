using System.Collections;

namespace Archetype.Framework.Runtime;

public class QueueStack<T>
{
    private readonly List<T> _items = new();

    public int Count => _items.Count;
    
    public void Enqueue(T item)
    {
        _items.Add(item);
    }
    
    public void Push(T item)
    {
        _items.Insert(0, item);
    }
    
    public T Pop()
    {
        var item = _items[0];
        _items.RemoveAt(0);
        return item;
    }
}