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
    }
}