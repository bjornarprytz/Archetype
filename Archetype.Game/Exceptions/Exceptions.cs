using System;
using System.Collections;
using Archetype.Game.Payloads.Context.Card;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Exceptions
{
    public class MalformedEffectException : Exception
    {
        public override string Message { get; }

        public MalformedEffectException(string message)
        {
            Message = message;
        }
    }

    public class EffectResultMissingVerbException : Exception { }
    public class CardMissingFromResolutionException : Exception { }
    public class TargetsMissingFromResolutionException : Exception { }
    
    public class ContextResolvedTwiceException : Exception
    {
        public override string Message { get; }

        public ContextResolvedTwiceException(ICard card, CardResolutionContext cardResolutionContext)
        {
            Message = $"Trying to resolve {card.MetaData.Name} on a context which has already been resolved {cardResolutionContext}";
        }
    }
    
    public class InvalidTargetChosenException : Exception
    {
        public InvalidTargetChosenException()
        {
            
        }
    }
    public class TargetCountMismatchException : Exception
    {
        public override string Message { get; }

        public TargetCountMismatchException(int expected, int actual)
        {
            Message = $"Expected {expected} targets, but got {actual} targets";
        }
    }

    public class PlayCardException : Exception
    {
        public override string Message { get; }

        public PlayCardException(string message)
        {
            Message = message;
        }
    }

    public class StartGameException : Exception
    {
        public override string Message { get; }

        public StartGameException(string message)
        {
            Message = message;
        }
    }
    
    public class MovePhaseException : Exception
    {
        public override string Message { get; }

        public MovePhaseException(string message)
        {
            Message = message;
        }
    }
}