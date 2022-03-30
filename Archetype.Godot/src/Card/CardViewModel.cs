using Godot;
using System;
using System.Linq;
using Archetype.Godot.Extensions;
using Archetype.Prototype1Data;

public class CardViewModel : Control
{
	public void Load(ICard cardData)
	{
		GetNode<Label>("Panel/Name").Text = cardData.Name;
		GetNode<Label>("Panel/Cost").Text = cardData.Cost.ToString();
		GetNode<Label>("Panel/RulesText").Text = cardData.Keywords.IsEmpty() ? "" : cardData.Keywords
			.Select(keyword => keyword.ToString()).Aggregate((keyword, keyword1) => $"{keyword}, {keyword1}");
			
		GetNode<Label>("Panel/Defense/Value").Text = cardData.Health.ToString();
		GetNode<Label>("Panel/Attack/Value").Text = cardData.Strength.ToString();
		GetNode<Label>("Panel/Presence/Value").Text = cardData.Presence.ToString();
	}
}
