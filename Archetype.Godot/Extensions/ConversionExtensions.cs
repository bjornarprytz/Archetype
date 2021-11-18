using System;
using Archetype.Client;
using Godot;

namespace Archetype.Godot.Extensions
{
    public static class ConversionExtensions
    {
        public static Color ToGodot(this CardColor cardColor)
        {
            return (cardColor) switch
            {
                CardColor.White => Colors.White,
                CardColor.Blue => Colors.Blue,
                CardColor.Black => Colors.Black,
                CardColor.Red => Colors.Red,
                CardColor.Green => Colors.Green,
                _ => throw new ArgumentOutOfRangeException(nameof(cardColor), cardColor, null)
            };
        }
    }
}