using System;
using System.Reactive.Disposables;
using Archetype.Godot.Card;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;
using Godot;
using Godot.Collections;

public class Hand : Area2D
{
	private readonly CompositeDisposable _disposable = new();
	private IGameView _gameView;
	private ICardFactory _cardFactory;


	private readonly Dictionary<ICard, CardNode> _cards = new();
	public override void _ExitTree()
	{
		base._ExitTree();
		_disposable.Dispose();
	}

	[Inject]
	public void Construct(IGameView gameView, ICardFactory cardFactory)
	{
		_gameView = gameView;

		gameView.GameState.Player.OnCardDrawn
			.Subscribe(OnCardDrawn)
			.AddTo(_disposable);
		
		gameView.GameState.Player.OnCardRemoved
			.Subscribe(OnCardRemoved)
			.AddTo(_disposable);

		_cardFactory = cardFactory;

	}

	private void OnCardDrawn(ICard card)
	{
		var cardNode = _cardFactory.CreateCard(card);
		_cards.Add(card, cardNode);
		
		AddChild(cardNode);
	}
	
	private void OnCardRemoved(ICard card)
	{
		var cardNode = _cards[card];
		_cards.Remove(card);
		
		RemoveChild(cardNode);
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
}
