using Archetype.Framework.Core.Primitives;

namespace Archetype.Framework.BaseRules.Keywords;

public static class Prompt
{
    public static IEffectResult PickBetweenNandM(IResolutionContext context, Guid promptId, IAtomProvider atomProvider, int min, int max, string promptText)
    {
        var atoms = atomProvider.ProvideAtoms(context).ToList();

        var atomIds = atoms.Select(a => a.Id).ToList();

        return new PromptDescription(promptId, atomIds, min, max, promptText);
    }
    
    public static IEffectResult PickN(IResolutionContext context, Guid promptId, IAtomProvider atomProvider, int n, string promptText)
    {
        var atoms = atomProvider.ProvideAtoms(context).ToList();

        var atomIds = atoms.Select(a => a.Id).ToList();

        return new PromptDescription(promptId, atomIds, n, n, promptText);
    }
    
    public static IEffectResult PickOne(IResolutionContext context, Guid promptId, IAtomProvider atomProvider, string promptText)
    {
        var atoms = atomProvider.ProvideAtoms(context).ToList();

        var atomIds = atoms.Select(a => a.Id).ToList();

        return new PromptDescription(promptId, atomIds, 1, 1, promptText);
    }
}