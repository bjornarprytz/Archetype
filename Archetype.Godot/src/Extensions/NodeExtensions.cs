using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Archetype.Godot.Extensions
{
    public static class NodeExtensions
    {
        public static IEnumerable<T> GetChildren<T>(this Node node) 
            where T : Node
        {
            return node.GetChildren().Cast<Node>()
                .Select(child => child as T)
                .Where(child => child is not null);
        }
        
        public static T GetRequiredChild<T>(this Node node) 
            where T : Node
        {
            return node.GetChildren<T>().First();
        }

        public static RayHitResult CastRayFromMousePosition(this Spatial spatial, Vector2 mouseScreenPosition, bool collideWithAreas=false, bool collideWithBodies=true, float range=50f)
        {
            var spaceState = spatial.GetWorld().DirectSpaceState;
            var camera = spatial.GetViewport().GetCamera();
            var from = camera.ProjectRayOrigin(mouseScreenPosition);
            var to = from + (camera.ProjectRayNormal(mouseScreenPosition) * range); 
			
            return new RayHitResult(spaceState.IntersectRay(from, to, collideWithAreas: collideWithAreas, collideWithBodies: collideWithBodies));
        }
    }
}