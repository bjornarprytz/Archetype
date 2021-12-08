using Archetype.Game.Exceptions;
using Archetype.Game.Extensions;
using Archetype.Game.Payloads.Infrastructure;
using Archetype.Game.Payloads.Pieces;

namespace Archetype.Game.Payloads.Context.Phase
{
    public interface IPhaseResolver
    {
        string Name { get; }
        void Resolve();
    }
    
    public interface IMovePhaseResolver : IPhaseResolver 
    { }

    public class MovePhaseResolver : PhaseResolver, IPhaseResolver
    {
        private readonly IGameState _gameState;
        

        public MovePhaseResolver(IGameState gameState, IHistoryWriter historyWriter) : base(historyWriter)
        {
            _gameState = gameState;
        }

        public override string Name => "Move Phase";

        protected override IResolution ResolvePhase()
        {
            var resultsCollector = new ResolutionCollector();

            var shortestPaths = (_gameState.Player.HeadQuarters.CurrentZone as IMapNode).CalculateShortestPaths();
            
            foreach (var enemy in _gameState.Map.EachEnemyCreature())
            {
                for (var i = 0; i < enemy.Movement; i++)
                {
                    if (enemy.CurrentZone is not IMapNode node)
                        throw new MovePhaseException($"Enemy {enemy} must be on the map");

                    if (node == _gameState.Player.HeadQuarters.CurrentZone)
                        break;

                    if (node.CalculateDefenseAgainst(enemy) > enemy.Strength)
                        continue;
                    
                    resultsCollector.AddResult(enemy.MoveTo(shortestPaths[node]));
                }
            }

            return resultsCollector;
        }
    }

    public abstract class PhaseResolver : IPhaseResolver
    {
        private readonly IHistoryWriter _historyWriter;

        protected PhaseResolver(IHistoryWriter historyWriter)
        {
            _historyWriter = historyWriter;
        }

        public abstract string Name { get; }

        public void Resolve()
        {
            _historyWriter.Append(this, ResolvePhase());
        }

        protected abstract IResolution ResolvePhase();
    }
}