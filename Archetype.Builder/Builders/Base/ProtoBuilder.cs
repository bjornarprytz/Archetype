using Archetype.Builder.Exceptions;
using Archetype.Builder.Extensions;
using Archetype.Game.Payloads.Proto;

namespace Archetype.Builder.Base
{
    public abstract class ProtoBuilder<T> : IBuilder<T>
        where T : IProtoData
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