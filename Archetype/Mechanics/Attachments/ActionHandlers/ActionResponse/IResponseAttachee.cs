using System;
using System.Collections.Generic;

namespace Archetype
{
    public interface IResponseAttachee<THost>
        where THost : class, IResponseAttachee<THost>
    {
        List<ActionResponse<THost>> Responses { get; }

        void AttachResponse(ActionResponse<THost> response)
        {
            response.AttachHandler(this as THost);
            Responses.Add(response);
        }
        void DetachResponse(ActionResponse<THost> response)
        {
            Responses.Remove(response);
            response.DetachHandler(this as THost);
        }
    }
}
