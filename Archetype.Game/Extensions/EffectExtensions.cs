using Aqua.TypeSystem;
using Archetype.Core;

namespace Archetype.Game.Extensions
{
    public static class EffectExtensions
    {

        public static bool CallTargetValidationMethod(this IEffectMetaData effectMetaData, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic validationLambda = effectMetaData.GetType().GetProperty(effectMetaData.ValidationFunctionName)?.GetValue(effectMetaData);

            dynamic dynGamePiece = gamePiece;

            return validationLambda(dynGamePiece, gameState);
        }
        
        public static object CallResolveMethod(this IEffectMetaData effectMetaData, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic resolutionLambda = effectMetaData.GetType().GetProperty(effectMetaData.ResolutionFunctionName)?.GetValue(effectMetaData);

            dynamic dynGamePiece = gamePiece;

            return resolutionLambda(dynGamePiece, gameState);
        }
    }
}