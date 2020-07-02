namespace Archetype
{
    public interface IResponseAttachee<THost>
    {
        void AttachResponse(ActionResponse<THost> response);
        void DetachResponse(ActionResponse<THost> response);
    }
}
