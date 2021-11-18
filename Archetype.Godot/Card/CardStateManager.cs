using Godot;
using Archetype.Godot.Card;
using Archetype.Godot.Targeting;

public class CardStateManager : Node
{
	private readonly HighlightStateMachine<CardNode> _highlightStateMachine;
	private readonly TargetingStateMachine<CardNode> _targetingStateMachine;
	
	public CardStateManager(CardNode card)
	{
		_highlightStateMachine = new HighlightStateMachine<CardNode>(card);
		_targetingStateMachine = new TargetingStateMachine<CardNode>(card);
	}
	
	public override void _Ready()
	{
		AddChild(_highlightStateMachine);
		_highlightStateMachine.Owner = this;
		AddChild(_targetingStateMachine);
		_targetingStateMachine.Owner = this;
	}
}
