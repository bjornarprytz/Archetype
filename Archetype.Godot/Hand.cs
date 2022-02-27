using System;
using System.Reactive.Disposables;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Archetype.Prototype1Data;
using Godot;

public class Hand : Area2D
{
	private readonly CompositeDisposable _disposable = new();
	private IGameView _gameView;
	private ICardFactory _cardFactory;

	public override void _ExitTree()
	{
		base._ExitTree();
		_disposable.Dispose();
	}

	[Inject]
	public void Construct(IGameView gameView, ICardFactory cardFactory)
	{
		_gameView = gameView;

		// TODO : subscribe to OnCardRemoved in hand

		gameView.GameState.Player.OnCardDrawn
			.Subscribe(OnCardDrawn)
			.AddTo(_disposable);

		_cardFactory = cardFactory;

	}

	private void OnCardDrawn(ICard card)
	{
		AddChild(_cardFactory.CreateCard(card));
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}
}
