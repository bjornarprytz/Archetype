using System;

namespace Archetype.Godot.Infrastructure
{
    public class MissingPackedSceneException : Exception
    {
        public override string Message { get; }

        public MissingPackedSceneException(string scenePath)
        {
            Message = scenePath;
        }
    }
}