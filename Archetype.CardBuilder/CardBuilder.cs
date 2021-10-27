using Archetype.Core;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Archetype.CardBuilder.Extensions;

namespace Archetype.CardBuilder
{
    public class CardBuilder : BaseBuilder<CardData>
    {
        private StringBuilder _rulesTextBuilder = new ();
        
        internal CardBuilder(CardData template=null) : base(() => template ?? new CardData{ Id = Guid.NewGuid() })
        {
        }

        

        public CardBuilder Name(string name)
        {
            Construction.Name = name;

            return this;
        }

        public CardBuilder Rarity(CardRarity rarity)
        {
            Construction.Rarity = rarity;

            return this;
        }

        public CardBuilder Cost(int cost)
        {
            Construction.Cost = cost;

            return this;
        }

        public CardBuilder Color(CardColor color)
        {
            Construction.Color = color;

            return this;
        }

       

        public CardBuilder Text(string text)
        {
            _rulesTextBuilder = new StringBuilder(text);

            return this;
        }
        
        public CardBuilder AddTextLine(string text)
        {
            _rulesTextBuilder.AppendLine(text);

            return this;
        }

        public CardBuilder Art(string link)
        {
            Construction.ImageUri = link;

            return this;
        }

        public CardBuilder Target<TTarget>(Func<TTarget, IGameState, bool> validateTarget=null)
            where TTarget : IGamePiece
        {
            Construction.TargetData.Add(new TargetData<TTarget>(validateTarget));

            return this;
        }
        public CardBuilder Targets<TT1, TT2>(
            Func<TT1, IGameState, bool> validateTarget1=null, 
            Func<TT2, IGameState, bool> validateTarget2=null
        )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
        {
            
            Construction.TargetData.Add(new TargetData<TT1>(validateTarget1));
            Construction.TargetData.Add(new TargetData<TT2>(validateTarget2));

            return this;
        }
        public CardBuilder Targets<TT1, TT2, TT3>(
            Func<TT1, IGameState, bool> validateTarget1=null, 
            Func<TT2, IGameState, bool> validateTarget2=null, 
            Func<TT3, IGameState, bool> validateTarget3=null 
            )
            where TT1 : IGamePiece
            where TT2 : IGamePiece
            where TT3 : IGamePiece
        {
            
            Construction.TargetData.Add(new TargetData<TT1>(validateTarget1));
            Construction.TargetData.Add(new TargetData<TT2>(validateTarget2));
            Construction.TargetData.Add(new TargetData<TT3>(validateTarget3));

            return this;
        }
        
        public CardBuilder EffectBuilder<TTarget, TResult>(Action<EffectBuilder<TTarget, TResult>> builderProvider)
            where  TTarget : IGamePiece
        {
            var cbc = BuilderFactory.EffectBuilder<TTarget, TResult>(); // Input template data here

            builderProvider(cbc);
            
            Construction.Effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder EffectBuilder<TResult>(Action<EffectBuilder<TResult>> builderProvider)
        {
            var cbc = BuilderFactory.EffectBuilder<TResult>(); // Input template data here

            builderProvider(cbc);
            
            Construction.Effects.Add(cbc.Build());

            return this;
        }
        
        public CardBuilder Effect<TTarget, TResult>(
            int targetIndex,
            Func<TTarget, IGameState, TResult> resolveEffect,
            Func<TTarget, IGameState, string> rulesText=null
            )
            where  TTarget : IGamePiece
        {
            return EffectBuilder<TTarget, TResult>(provider => 
                provider
                    .TargetIndex(targetIndex)
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? ((_, _) => string.Empty) )
                );
        }
        
        public CardBuilder Effect<TResult>(
            Func<IGameState, TResult> resolveEffect,
            Func<IGameState, string> rulesText=null
        )
        {
            return EffectBuilder<TResult>(provider => 
                provider
                    .Resolve(resolveEffect)
                    .Text(rulesText ?? (_ => string.Empty) )
            );
        }

        protected override void PreBuild()
        {
            var targetCount = Construction.TargetData.Count;
            
            foreach (var effect in Construction.Effects)
            {
                string textLine;

                try
                {
                    textLine = effect.CallTextMethod(null, null);
                }
                catch
                {
                    textLine = "Text error: Additional state needed";
                }
                
                AddTextLine(textLine);

                if (effect.TargetIndex >= targetCount)
                {
                    throw new InvalidTargetIndexException(effect.TargetIndex, targetCount);
                }
            }

            Construction.RulesText = _rulesTextBuilder.ToString();
            
            Console.WriteLine($"Creating card {Construction.Name}");
        }
    }
}
