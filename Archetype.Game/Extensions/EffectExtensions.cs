using Aqua.TypeSystem;
using Archetype.Core;

namespace Archetype.Game.Extensions
{
    public static class EffectExtensions
    {

        public static bool CallTargetValidationMethod(this IEffect effect, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic validationLambda = effect.GetType().GetProperty("Validate").GetValue(effect);

            dynamic dynGamePiece = gamePiece;

            return validationLambda(dynGamePiece, gameState);
        }
        
        public static object CallResolveMethod(this IEffect effect, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic resolutionLambda = effect.GetType().GetProperty("Resolve").GetValue(effect);

            dynamic dynGamePiece = gamePiece;

            return resolutionLambda(dynGamePiece, gameState);
        }
    }
}