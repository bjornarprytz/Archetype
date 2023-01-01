using Archetype.Core.Effects;
using Archetype.Core.Infrastructure;
using FluentValidation;
using MediatR;

namespace Archetype.Rules.Actions;

public class AnswerPrompt
{
    public record Command(List<Guid> Atoms) : IRequest<IActionResult>;

    public class Handler : IRequestHandler<Command, IActionResult>
    {
        private readonly IGameState _gameState;
        private readonly IPromptQueue _promptQueue;
        private readonly IAtomFinder _atomFinder;

        public Handler(IGameState gameState, IPromptQueue promptQueue, IAtomFinder atomFinder)
        {
            _gameState = gameState;
            _promptQueue = promptQueue;
            _atomFinder = atomFinder;
        }
        
        public Task<IActionResult> Handle(Command request, CancellationToken cancellationToken)
        {
            var atoms = request.Atoms.Select(_atomFinder.FindAtom).ToList();

            var promptResolver = _promptQueue.Dequeue();

            var results = new List<IResult>
            {
                promptResolver.Resolve(_gameState, atoms)
            };

            return Task.FromResult<IActionResult>(new ActionResult(results));
        }
    }
    
    public class Validator : AbstractValidator<Command>
    {
        public Validator(IGameState gameState, IAtomFinder atomFinder)
        {
            RuleFor(x => x)
                .Must(x => gameState.Prompter.CurrentPrompt != null)
                .WithMessage("No prompt is currently active.");
            
            RuleFor(x => x.Atoms)
                .Must(x => x.Count == x.Distinct().Count())
                .WithMessage("Answers must be unique.");
            
            RuleFor(x => x.Atoms)
                .Must(atoms => atoms.Select(atomFinder.FindAtom).All(atom => atom.GetType() == gameState.Prompter.CurrentPrompt?.AtomType))
                .WithMessage("Answers must be of the correct type.");
            
            RuleFor(x => x.Atoms.Count)
                .Must(count => count >= gameState.Prompter.CurrentPrompt?.MinAnswers)
                .Must(count => count <= gameState.Prompter.CurrentPrompt?.MaxAnswers)
                .WithMessage("The number of answers is not within the range of the prompt.");

            RuleFor(x => x.Atoms)
                .Must(answers => answers.All(answer => gameState.Prompter.CurrentPrompt?.EligibleAtoms.Contains(answer) ?? false))
                .WithMessage("One or more answers are not eligible for the current prompt.");
            
        }
    }
}