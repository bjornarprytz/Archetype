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
	}
}
