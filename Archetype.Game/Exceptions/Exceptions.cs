using System;
using Archetype.Game.Payloads.Atoms.Base;
using Archetype.Game.Payloads.Context.Card;
using Archetype.View.Atoms.Zones;

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

    public class ZonePlacementException : Exception
    {
        public override string Message { get; }

        internal ZonePlacementException(IZoneFront zone, string details)
        {
            Message = $"Invalid placement in zone {zone} due to: {details}";
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

    public class StartGameException : Exception
    {
        public override string Message { get; }

        public StartGameException(string message)
        {
            Message = message;
        }
    }
}