using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using Archetype.Godot.Card;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;
using Godot;

public class HandController : Path
{
	private readonly CompositeDisposable _disposable = new();
	private IGameView _gameView;
	private ICardFactory _cardFactory;


	private readonly Dictionary<Guid, CardNode> _cards = new();
	public override void _ExitTree()
	{
		base._ExitTree();
		_disposable?.Dispose();
	}

	public override void _Ready()
	{
		Curve.AddPoint(Vector3.Zero);
		Curve.AddPoint(Vector3.One);
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
		_cards[card.Id] = cardNode;
		
		AddChild(cardNode);
		ReSpaceCards();
	}
	
	private void OnCardRemoved(ICard card)
	{
		var cardNode = _cards[card.Id];
		_cards.Remove(card.Id);
		
		RemoveChild(cardNode);
		ReSpaceCards();
	}

	private void ReSpaceCards()
	{
		var anchors = Curve.GetBakedPoints().PolySect(_cards.Count).ToArray(); 
		
		foreach (var (cardNode, pos) in _cards.Values.Zip(anchors, (node, vector2) => (node, vector2)))
		{
			cardNode.Translation = pos;
		}
	}
}
