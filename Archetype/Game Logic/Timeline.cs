﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Archetype
{
    public class Timeline
    {
        public int CurrentTick { get; private set; }
        public List<EffectSpan> Effects;
        public List<Unit> InitiativeOrder; // Ordered on speed?

        public Timeline(int startTick=0)
        {
            CurrentTick = startTick;
            Effects = new List<EffectSpan>();
            InitiativeOrder = new List<Unit>();
        }

        public void AdvanceTime()
        {
            CurrentTick++;
        }

        public List<Tick> FutureTicks(int nTicksForward)
        {
            List<Tick> futureTicks = new List<Tick>();

            for (int i = 0; i < nTicksForward; i++)
            {
                futureTicks.Add(new Tick(CurrentTick, Effects));
            }

            return futureTicks;
        }

        public void ResolveEffects(DecisionPrompt prompt)
        {
            foreach (EffectSpan span in Effects)
            {
                span.ResolveTick(CurrentTick, prompt);
            }
        }

        public void CommitEffectSpan(EffectSpan span)
        {
            Effects.Add(span);
            span.StartTime = CurrentTick;
        }

        public void ResolveTurns(GameState gameState, DecisionPrompt prompt)
        {
            InitiativeOrder = gameState.ActiveUnits.OrderByDescending(u => u.Speed).ToList();

            foreach (Unit unit in InitiativeOrder)
            {
                if (unit.HasTurn(CurrentTick))
                    unit.TakeTurn(this, gameState, prompt);
            }
        }
    }
}