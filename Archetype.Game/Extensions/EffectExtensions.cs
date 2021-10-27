using System.Collections.Generic;
using Aqua.TypeSystem;
using Archetype.Core;

namespace Archetype.Game.Extensions
{
    public static class EffectExtensions
    {
        public static bool CallTargetValidationMethod(this ITargetMetaData targetMetaData, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic validationLambda = targetMetaData.GetType().GetProperty(targetMetaData.ValidationFunctionName)?.GetValue(targetMetaData);

            dynamic dynGamePiece = gamePiece;

            return validationLambda(dynGamePiece, gameState);
        }
        
        public static object CallResolveMethod(this IEffectMetaData effectMetaData, IList<IGamePiece> availableTargets, IGameState gameState)
        {
            dynamic resolutionLambda = effectMetaData.GetType().GetProperty(effectMetaData.ResolutionFunctionName)?.GetValue(effectMetaData);

            var target = effectMetaData.TargetType != default ? availableTargets[effectMetaData.TargetIndex] : null;
            
            if (target == null)
            {
                return resolutionLambda(gameState);
            }
            else
            {
                dynamic dynGamePiece = target;
                return resolutionLambda(dynGamePiece, gameState);
            }
        }
    }
}