using Godot;
using Archetype.Godot.Card;

public class CardStateManager : Node
{
	private readonly HighlightStateMachine<CardNode> _highlightStateMachine;
	
	public CardStateManager(CardNode card)
	{
		_highlightStateMachine = new HighlightStateMachine<CardNode>(card);
	}
	
	public override void _Ready()
	{
		AddChild(_highlightStateMachine);
	}
}
