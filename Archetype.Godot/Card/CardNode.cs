using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using Archetype.Client;
using Archetype.Godot.Extensions;
using Archetype.Godot.Infrastructure;
using Godot;
using Archetype.Godot.Targeting;

namespace Archetype.Godot.Card
{
	public class CardNode : Area2D, ICard
	{
		private CompositeDisposable _disposables = new ();
		private readonly Subject<bool> _onHovered = new();
		private readonly Subject<InputEventMouseButton> _onClick = new();
		private readonly Subject<IPlayCardContext> _onPlay = new();
		private CardStateManager _stateManager;
		private ICardProtoData _protoData;
		private IArchetypeGraphQLClient _client;

		public IObservable<bool> OnHover => _onHovered;
		public IObservable<InputEventMouseButton> OnClick => _onClick;
		public IObservable<IPlayCardContext> OnPlay => _onPlay;
		public Area2D TargetNode => this;

		public CardNode()
		{
			_stateManager = new CardStateManager(this);
		}
		
		public void Load(ICardProtoData protoData)
		{
			_protoData = protoData;
		}
		
		public override void _Ready()
		{
			base._Ready();
			
			Connect(Signals.CollisionObject2D.InputEvent, this, nameof(OnInputEvent));
			Connect(Signals.CollisionObject2D.MouseEntered, this, nameof(OnMouseEntered));
			Connect(Signals.CollisionObject2D.MouseExited, this, nameof(OnMouseExited));


			if (_protoData != null)
			{
				var name = GetNode("CardName") as RichTextLabel;
				name.Text = _protoData?.MetaData?.Name;
				var color = GetNode("ColorRect") as ColorRect;
				color.Color = _protoData.MetaData.Color.ToGodot();
				var cost = GetNode("CardCost") as RichTextLabel;
				cost.Text = _protoData.Cost.ToString();
				var text = GetNode("RulesText") as RichTextLabel;
				text.Text = _protoData.RulesText;
			}
			
			
			AddChild(_stateManager);
			_stateManager.Owner = this;
		}

		[Inject]
		public void Construct(IArchetypeGraphQLClient client)
		{
			_client = client;

			_client
				.OnGameStarted
				.Watch()
				.Subscribe((result => GD.Print(result?.Data?.OnGameStarted.Message)))
				.DisposeWith(_disposables);
		}
		
		
		public void HandleTarget(ITargetable target)
		{
			_onPlay.OnNext(null); // TODO: Validate target and send good data instead
		}

		private void OnInputEvent(object viewport, object @event, int shape_idx)
		{
			if (@event is InputEventMouseButton mb)
			{
				_onClick.OnNext(mb);
			}
		}
		
		private void OnMouseEntered()
		{
			_onHovered.OnNext(true);
		}

		private void OnMouseExited()
		{
			_onHovered.OnNext(false);
		}
	}
}
