using Aqua.TypeExtensions;
using Archetype.Core;
using Archetype.Core.Atoms;
using Archetype.Core.Atoms.Infrastructure;
using Archetype.Core.Effects;
using Archetype.Core.Effects.Resolution;
using Archetype.Core.Extensions;
using Archetype.Core.Infrastructure;
using Archetype.Core.Proto;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Encounter;

public class PlayCard
{
    public class Validator : AbstractValidator<Args>
    {
        public Validator()
        {
            RuleFor(a => a.CardId).Must((args, id) => args.PaymentCardIds.All(paymentId => paymentId != id))
                .WithMessage("Cannot pay with the card you are playing.");
            
            RuleFor(a => a.CardId).Must((args, id) => args.TargetGuids.All(targetGuid => targetGuid != id))
                .WithMessage("Cannot target the card you are playing.");
            
            RuleFor(a => a.PaymentCardIds).Must((paymentCardIds) => paymentCardIds.Count() == paymentCardIds.Distinct().Count())
                .WithMessage("Cannot pay with the same card twice.");

            RuleFor(a => a.TargetGuids).Must((targetGuids) => targetGuids.Count() == targetGuids.Distinct().Count())
                .WithMessage("Cannot target the same card twice.");
        }
    }
    public record Args(Guid CardId, List<Guid> PaymentCardIds, List<Guid> TargetGuids) : IRequest<Unit>;

    public class Handler : IRequestHandler<Args, Unit>
    {
        private readonly IGameState _gameState;
        private readonly IAtomFinder _atomFinder;
        private readonly IProtoFinder _protoFinder;

        public Handler(IGameState gameState, IAtomFinder atomFinder, IProtoFinder protoFinder)
        {
            _gameState = gameState;
            _atomFinder = atomFinder;
            _protoFinder = protoFinder;
        }
        
        public Task<Unit> Handle(Args request, CancellationToken cancellationToken)
        {
            var card = _atomFinder.FindAtom<ICard>(request.CardId);
            var paymentCards = request.PaymentCardIds.Select(_atomFinder.FindAtom<ICard>);
            var targets = request.TargetGuids.Select(_atomFinder.FindAtom);

            var protoCard = _protoFinder.FindProto<IProtoPlayingCard>(card.ProtoId);

            var context = protoCard switch {
                IProtoSpell spell => new EffectContext(_gameState, card, spell, new TargetProvider(targets, spell.TargetDescriptors)),
                IProtoCrew crew => throw new NotImplementedException(),
                IProtoStructure structure => throw new NotImplementedException(),
                _ => throw new ArgumentException("Unknown card type")
            };
            
            // TODO: Resolve context
            // TODO: Resolve payment
            // TODO: Resolve targets
            // TODO: Resolve triggers?
        }
    }

    private record EffectContext(IGameState GameState, IAtom Source, IEffectProvider EffectProvider,
        ITargetProvider TargetProvider) : IEffectContext
    {
        public static IEffectContext Create (IGameState gameState, IAtom source, IEffectProvider effectProvider, ITargetProvider targetProvider)
        {
            return new EffectContext(gameState, source, effectProvider, targetProvider);
        }
    }
    
    private class TargetProvider : ITargetProvider
    {
        private readonly Dictionary<Type, List<IAtom>> Targets = new();

        public TargetProvider(IEnumerable<IAtom> chosenTargets, IEnumerable<ITargetDescriptor> targetDescriptors)
        {
            var required = targetDescriptors.ToList();
            var chosen = chosenTargets.ToList();
        
            if (required.Count != chosen.Count)
            {
                throw new ArgumentException($"Number of targets ({chosen.Count}) does not match required number of targets ({required.Count})");
            }
            
            foreach (var (targetData, chosenTarget) in required.Zip(chosen))
            {
                var targetType = targetData.TargetType;
            
                if (!chosenTarget.GetType().Implements(targetType))
                {
                    throw new ArgumentException("Target does not match required target type");
                }
            
                Targets.GetOrSet(targetType).Add(chosenTarget);
            }
        }
        public T GetTarget<T>() where T : IAtom
        {
            return GetTargetInternal<T>(0);
        }

        public T GetTarget<T>(int index) where T : IAtom
        {
            return GetTargetInternal<T>(index);
        }

        private T GetTargetInternal<T>(int index)
        {
            var targetsOfTypeT = Targets[typeof(T)];

            if (targetsOfTypeT.IsEmpty() || targetsOfTypeT.Count <= index)
            {
                throw new Exception($"Can't provide target of type {typeof(T)} and index {index}");
            }
        
            return (T) targetsOfTypeT[index];
        }
    }
}