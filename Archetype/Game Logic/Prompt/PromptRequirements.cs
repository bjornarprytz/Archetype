﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Archetype
{
    public class PromptRequirements
    {
        public Type RequiredType { get; private set; }
        public int Max { get; private set; }
        public int Min { get; private set; }
        internal string TargetsText => $"{_numberOfTargets} {_targetsFaction} {_typeTargets}";

        private Faction _allowedFactions;

        public PromptRequirements(int x, Type t, Faction allowedFactions)
        {
            if (!t.IsSubclassOf(typeof(GamePiece))) throw new Exception("Can only require Game Piece types in prompts");

            RequiredType = t;
            Min = Max = x;

            _allowedFactions = allowedFactions;
        }

        public PromptRequirements(int min, int max, Type t, Faction allowedFactions)
        {
            if (!t.IsSubclassOf(typeof(GamePiece))) throw new Exception("Can only require Game Piece types in prompts");

            RequiredType = t;
            Min = min;
            Max = max;

            _allowedFactions = allowedFactions;
        }


        public bool MeetsRequirements(GamePiece piece)
        {
            if (piece.GetType() != RequiredType) return false;
            if (!_allowedFactions.HasFlag(piece.Team)) return false;


            return true;
        }

        private string _numberOfTargets => Max == Min ? $"{Max}" : $"{Min}-{Max}";
        private string _targetsFaction
        {
            get
            {
                switch (_allowedFactions)
                {
                    case Faction.Enemy:
                        return "enemy";
                    case Faction.Player:
                        return "friendly";
                    case Faction.Neutral:
                        return "neutral";
                    case Faction.Any:
                    default:
                        return string.Empty;
                }
            }
        }
        private string _typeTargets => $"{RequiredType}";
    }
}