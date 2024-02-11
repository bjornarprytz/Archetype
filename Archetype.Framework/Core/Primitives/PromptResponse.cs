using Archetype.Framework.State;

namespace Archetype.Framework.Core.Primitives;

public interface IPromptResponse
{
    IPromptDescription Description { get; }
    bool IsPending { get; }
    IReadOnlyList<IAtom> Selection { get; }
    
    void Answer(IReadOnlyList<IAtom> selection);
}

public class PromptResponse : IPromptResponse
{
    private readonly List<IAtom> _selection = new();

    private PromptResponse(IPromptDescription description)
    {
        Description = description;
    }

    public IReadOnlyList<IAtom> Selection => _selection;
    public void Answer(IReadOnlyList<IAtom> selection)
    {
        // TODO: Validate somewhere in middleware
        if (!IsPending)
        {
            throw new InvalidOperationException("Prompt response has already been answered.");
        }
        
        if (Description.MinPicks > selection.Count || Description.MaxPicks < selection.Count)
        {
            throw new InvalidOperationException("Invalid number of selections.");
        }
        
        if (selection.Any(s => !Description.Options.Contains(s.Id)))
        {
            throw new InvalidOperationException("Invalid selection.");
        }
        
        _selection.Clear();
        _selection.AddRange(selection);
        IsPending = false;
    }

    public IPromptDescription Description { get; }
    public bool IsPending { get; private set; } = true;
    
    public static IPromptResponse Create(IPromptDescription prompt)
    {
        return new PromptResponse(prompt);
    } 
    
    
    
}