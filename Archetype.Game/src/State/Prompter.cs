using Archetype.Core.Atoms;
using Archetype.Core.Infrastructure;

namespace Archetype.Game.State;

public class Prompter : IPrompter, IPromptQueue
{
    private readonly Queue<IPromptResolver> _prompts = new();

    public IPrompt? CurrentPrompt => _prompts.Any() ? _prompts.Peek() : null;

    public void Enqueue(IPromptResolver prompt)
    {
        _prompts.Enqueue(prompt);
    }

    public IPromptResolver Dequeue()
    {
        return _prompts.Dequeue();
    }
}