using Archetype.Core;

namespace Archetype.CardBuilder.Extensions
{
    public static class EffectExtensions
    {
        public static string CallTextMethod(this IEffectMetaData effectMetaData, IGamePiece gamePiece, IGameState gameState)
        {
            dynamic textLambda = effectMetaData.GetType().GetProperty(effectMetaData.RulesTextFunctionName)?.GetValue(effectMetaData);

            dynamic dynGamePiece = gamePiece;

            return textLambda(dynGamePiece, gameState);
        }
    }
}