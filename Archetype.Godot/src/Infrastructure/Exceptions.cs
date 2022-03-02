using System;

namespace Archetype.Godot.Infrastructure
{
	public class MissingPackedSceneException : Exception
	{
		public override string Message { get; }

		public MissingPackedSceneException(string scenePath)
		{
			Message = $"Missing scene at path: {scenePath}. Have you added it to the service collection?";
		}

		public MissingPackedSceneException(Type type)
		{
			Message = $"Missing scene for type: {type}. Have you added it to the service collection?";
		}

		public MissingPackedSceneException(Type expectedType, Type actualType)
		{
			Message = $"Missing scene for type: {expectedType}. Actual type: {actualType}. Have you added the wrong type to the service collection?";
			
		}
	}
}
