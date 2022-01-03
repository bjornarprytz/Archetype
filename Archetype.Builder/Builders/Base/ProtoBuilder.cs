using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Game.Payloads.Proto;
using Archetype.View.Proto;

namespace Archetype.Builder.Builders.Base
{
    public abstract class ProtoBuilder<T> : IBuilder<T>
        where T : IProtoDataFront
    {
        protected ProtoBuilder() { }
        
        public T Build()
        {
            var protoData = BuildInternal();

            if (protoData.Name.IsMissing())
                throw new MissingNameException();
            
            return protoData;
        }

        protected abstract T BuildInternal();
    }
}