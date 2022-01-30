using System.Runtime.CompilerServices;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context;

namespace Archetype.Game.Factory
{
    internal static class ResultFactory
    {
        public static IEffectResult<TAffected, TResult> Create<TAffected, TResult>(TAffected affected, TResult result, [CallerMemberName] string callerMemberName=default)
            where TAffected : class, IGameAtom
        {
            return new EffectEffectResult<TAffected, TResult>(affected, callerMemberName, result);
        }
        
        public static IEffectResult<TResult> Create<TResult>(TResult result, [CallerMemberName] string callerMemberName=default)
        {
            return new EffectEffectResult<TResult>(callerMemberName, result);
        }

        public static IEffectResult<TResult> Null<TResult>([CallerMemberName] string callerMemberName=default)
        {
            return new NullEffectResult<TResult>(callerMemberName);
        }
        
        public static IEffectResult<TAffected, TResult> Null<TAffected, TResult>(TAffected affected, [CallerMemberName] string callerMemberName=default) 
            where TAffected : class, IGameAtom
        {
            return new NullEffectResult<TAffected, TResult>(affected, callerMemberName);
        }
    }
}