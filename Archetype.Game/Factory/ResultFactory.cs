using System.Runtime.CompilerServices;
using Archetype.Game.Payloads.Pieces.Base;
using Archetype.Game.Payloads.PlayContext;

namespace Archetype.Game.Factory
{
    public static class ResultFactory
    {
        public static IEffectResult<TAffected, TResult> Create<TAffected, TResult>(TAffected affected, TResult result, [CallerMemberName] string callerMemberName=default)
            where TAffected : class, IGameAtom
        {
            return new EffectResult<TAffected, TResult>(affected, callerMemberName, result);
        }
        
        public static IEffectResult<TResult> Create<TResult>(TResult result, [CallerMemberName] string callerMemberName=default)
        {
            return new EffectResult<TResult>(callerMemberName, result);
        }
        
        public static IEffectResult<TResult> Null<TResult>([CallerMemberName] string callerMemberName=default)
        {
            return new NullResult<TResult>(callerMemberName);
        }
        
        public static IEffectResult<TAffected, TResult> Null<TAffected, TResult>(TAffected affected, [CallerMemberName] string callerMemberName=default) 
            where TAffected : class, IGameAtom
        {
            return new NullResult<TAffected, TResult>(affected, callerMemberName);
        }
    }
}