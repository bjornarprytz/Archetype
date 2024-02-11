using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IPromptResponse
{
    bool IsPending { get; }
    IReadOnlyList<IAtom> Selection { get; }
    
    void Answer(IEnumerable<IAtom> selection);
}

public class PromptResponse : IPromptResponse
{
    private readonly List<IAtom> _selection = new();
    
    private PromptResponse() { }

    public IReadOnlyList<IAtom> Selection => _selection;
    public void Answer(IEnumerable<IAtom> selection)
    {
        if (!IsPending)
        {
            throw new InvalidOperationException("Prompt response has already been answered.");
        }
        
        _selection.AddRange(selection);
        IsPending = false;
    }

    public bool IsPending { get; private set; } = true;
    
    public static IPromptResponse Pending { get; } = new PromptResponse();
    
    
    
}