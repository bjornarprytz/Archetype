// See https://aka.ms/new-console-template for more information

using Archetype.Play;

Console.WriteLine("Hello, World!");

var gameContext = Game.Create();

gameContext.BootStrap();

var setup = gameContext.Setup();

var node = setup.Map.Nodes.First();

var turn = setup.Start(node);

var card = turn.PlayableCards.First();

var playCardContext = turn.PlayCard(card);
	