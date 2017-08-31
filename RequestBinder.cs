namespace WebServer
{
    /// <summary>
    /// Holds bindings from local uri to responder.
    /// </summary>
    public class RequestBinder
    {
        /// <summary>
        /// Holds all GET methods.
        /// </summary>
        public readonly RequestMethodBinder Get = new RequestMethodBinder();

        /// <summary>
        /// Holds all POST methods.
        /// </summary>
        public readonly RequestMethodBinder Post = new RequestMethodBinder();
    }
}