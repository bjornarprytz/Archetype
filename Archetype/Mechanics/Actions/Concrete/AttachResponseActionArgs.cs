namespace Archetype
{
    public class AttachResponseActionArgs<THost> : ActionInfo
        where THost : class, ITarget, IResponseAttachee<THost>
    {
        public ActionResponse<THost> Response { get; set; }

        public AttachResponseActionArgs(ISource source, THost target, ActionResponse<THost> response) : base(source, target)
        {
            Response = response;
        }

        protected override void Resolve()
        {
            (Target as THost).AttachResponse(Response);
        }
    }
}
