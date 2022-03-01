using System.Collections.Generic;
using System.Linq;
using Archetype.Prototype1Data;
using Godot;

namespace Archetype.Godot.Extensions
{
    public static class NodeExtensions
    {
        public static IEnumerable<T> GetSubtree<T>(this Node node)
            where T : Node
        {
            var allChildren = new List<T>();

            var newChildren = new List<T>(node.GetChildren<T>());
            
            allChildren.AddRange(newChildren);

            foreach (var child in newChildren)
            {
                allChildren.AddRange(child.GetSubtree<T>());
            }

            return allChildren;
        }

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
    }
}