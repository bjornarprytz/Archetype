using Archetype.Core.Atoms;
using Archetype.Core.Infrastructure;

namespace Archetype.Core.Prompts;

public interface IPrompter
{
    IPrompt? CurrentPrompt { get; }
}

public interface IPromptQueue
{
    void Enqueue(IPromptResolver prompt);
    IPromptResolver Dequeue();

}

public interface IPromptResolver<in TAtom> : IPromptResolver 
    where TAtom : IAtom
{
    Type IPrompt.AtomType => typeof(TAtom);

    // TODO: This should also return some kind of result
    void Resolve(IGameState gameState, IEnumerable<TAtom> atoms);
    
}

public interface IPromptResolver : IPrompt
{
    // TODO: This should also return some kind of result
    // TODO: Find a way to make this generic so that we can pass in the correct type of atom.
    void Resolve(IGameState gameState, IEnumerable<IAtom> atoms);
}

public interface IPrompt
{
    public Type AtomType { get; }
    string PromptType { get; }
    IEnumerable<Guid> EligibleAtoms { get; }
    int MaxAnswers { get; }
    int MinAnswers { get; }
}