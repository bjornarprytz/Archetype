using System.Runtime.CompilerServices;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Factory
{
    public static class ResultFactory
    {
        public static IEffectResult<T> Create<T>(T target, int result, [CallerMemberName] string callerMemberName=default)
            where T : IGameAtom
        {
            return new EffectResult<T>(target, callerMemberName, result);
        }
        
        public static IEffectResult Create(int result, [CallerMemberName] string callerMemberName=default)
        {
            return new EffectResult(callerMemberName, result);
        }
    }
}