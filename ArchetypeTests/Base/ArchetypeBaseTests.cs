using System;
using System.Collections.Generic;
using Archetype;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ArchetypeTests
{
    [TestClass]
    public abstract class ArchetypeTestBase : IPrompter
    {
        protected GameState GameState { get; set; }
        protected HumanPlayer HumanPlayer { get; set; }
        protected EnemyPlayer EnemyPlayer { get; set; }

        protected List<object> ChoicesToMake { get; set; }

        protected CardData AttackCard { get; set; }
        protected CardData HealCard { get; set; }
        protected CardData CopyCard { get; set; }
        protected CardData TriggerCard { get; set; }

        protected Adventurer Friend1 { get; set; }
        protected Adventurer Friend2 { get; set; }

        protected Enemy Enemy1 { get; set; }
        protected Enemy Enemy2 { get; set; }
        protected Enemy Enemy3 { get; set; }

        [TestInitialize]
        public virtual void InitializeTests()
        {
            HumanPlayer = new HumanPlayer(69);
            EnemyPlayer = new EnemyPlayer();
            GameState = new GameState();

            ChoicesToMake = new List<object>();

            AttackCard = AttackAnEnemy(3);
            HealCard = SelfDamageAndHealAlly(1, 4);
            CopyCard = CopyACard();

            Friend1 = new Adventurer(HumanPlayer, GenerateUnitStats(15, 4), this);
            Friend2 = new Adventurer(HumanPlayer, GenerateUnitStats(7, 2), this);

            Enemy1 = new Enemy(EnemyPlayer, GenerateUnitStats(1, 0), this);
            Enemy2 = new Enemy(EnemyPlayer, GenerateUnitStats(2, 0), this);
            Enemy3 = new Enemy(EnemyPlayer, GenerateUnitStats(3, 0), this);


            GameState.AddUnits(new List<Unit>
            {
                Friend1,
                Friend2,
                Enemy1,
                Enemy2,
                Enemy3,
            });
        }

        protected TargetRequirementData AnyCardInAnEnemyHand()
        {
            return new TargetRequirementData
            {
                Predicate = new CardPredicateData
                {
                    CardZone = CardZone.Hand,
                    OwnerPredicate = new UnitPredicateData 
                    { 
                        Relation = TargetRelation.Enemy, 
                        UnitZone = UnitZone.Battlefield 
                    }
                },
                Selection = new AnySelectionData
                {
                    Min = 1,
                    Max = 1,
                },
            };
        }

        protected TargetRequirementData AllCardsInMyDiscardPile()
        {
            return new TargetRequirementData
            {
                Predicate = new CardPredicateData
                {
                    CardZone = CardZone.DiscardPile,
                    OwnerPredicate = new UnitPredicateData { Selfness = Selfness.Me }
                },
                Selection = new AllSelectionData(),
            };
        }

        protected TargetRequirementData AnyEnemies(int min, int max)
        {
            return new TargetRequirementData
            {
                Predicate = LivingEnemiesPredicate(),
                Selection = new AnySelectionData 
                { 
                    Min = min, 
                    Max = max,
                },
            };
        }

        protected TargetRequirementData Self()
        {
            return new TargetRequirementData
            {
                Predicate = new UnitPredicateData { Relation = TargetRelation.Ally, Selfness = Selfness.Me, UnitZone = UnitZone.Battlefield },
                Selection = new AllSelectionData(),
            };
        }

        protected TargetRequirementData AllEnemies()
        {
            return new TargetRequirementData
            {
                Predicate = LivingEnemiesPredicate(),
                Selection = new AllSelectionData(),
            };
        }

        protected TargetRequirementData RandomEnemies(int n)
        {
            return new TargetRequirementData
            {
                Predicate = LivingEnemiesPredicate(),
                Selection = new RandomSelectionData { Max = n },
            };
        }

        protected TargetRequirementData AnyAllies(int min, int max)
        {
            return new TargetRequirementData
            {
                Predicate = LivingAlliesPredicate(),
                Selection = new AnySelectionData 
                { 
                    Min = min, 
                    Max = max, 
                },
            };
        }

        protected UnitData GenerateUnitStats(int life, int resources)
        {
            return new UnitData
            {
                Name = "Place Holder",
                MaxLife = life,
                StartingResources = resources,
            };
        }

        protected TargetRequirementData AllDeadAllies()
        {
            return new TargetRequirementData
            {
                Predicate = DeadAlliesPredicate(),
                Selection = new AllSelectionData(),
            };
        }

        protected UnitPredicateData LivingEnemiesPredicate() => new UnitPredicateData
        {
            Relation = TargetRelation.Enemy,
            UnitZone = UnitZone.Battlefield,
        };
        
        protected UnitPredicateData LivingAlliesPredicate() => new UnitPredicateData
        {
            Relation = TargetRelation.Ally,
            UnitZone = UnitZone.Battlefield,
        };

        protected UnitPredicateData DeadAlliesPredicate() => new UnitPredicateData
        {
            Relation = TargetRelation.Ally,
            UnitZone = UnitZone.Graveyard,
        };

        protected CardData AttackAnEnemy(int damage)
        {
            return new CardData
            {
                Cost = 1,
                Actions = new List<ActionParameterData>
                {
                    new DamageParameterData
                    {
                        TargetRequirements = AnyEnemies(1, 1),
                        Strength = new ImmediateValue<int>(damage)
                    }
                }
            };
        }

        protected CardData SelfDamageAndHealAlly(int damage, int heal)
        {
            return new CardData
            {
                Cost = 2,
                Actions = new List<ActionParameterData>
                {
                    new DamageParameterData
                    {
                        TargetRequirements = new TargetRequirementData
                        {
                            Predicate = new UnitPredicateData{ Selfness = Selfness.Me, Relation = TargetRelation.Ally, UnitZone = UnitZone.Battlefield, },
                            Selection = new AllSelectionData(),
                        },
                        Strength = new ImmediateValue<int>(damage)
                    },
                    new HealParameterData
                    {
                        TargetRequirements = AnyAllies(1, 1),
                        Strength = new ImmediateValue<int>(heal)
                    }
                }
            };
        }

        protected CardData CopyACard()
        {
            return new CardData
            {
                Cost = 3,
                Actions = new List<ActionParameterData>
                {
                    new CopyCardParameterData
                    {
                        TargetRequirements = AnyCardInAnEnemyHand()
                    }
                }
            };
        }

        protected void InsertCardsIntoZone(int n, Unit owner, Zone<Card> zone)
        {
            for(var i = 0; i < n; i++)
            {
                AttackCard.MakeCopy(owner).MoveTo(zone);
            }
        }

        public virtual void Choose<T>(ISelectionInfo<T> selectionInfo)
        {
            if (selectionInfo.IsAutomatic) return;

            foreach (var choice in ChoicesToMake)
            {
                selectionInfo.Add((T)choice);
            }
        }
    }
}
