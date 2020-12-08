namespace OwnID.Extensibility.Flow.Contracts.Jwt
{
    /// <summary>
    ///     Wrapper for JWT base64 encoded string for transferring to OwnId application
    /// </summary>
    public class JwtContainer : ICommandResult
    {
        public JwtContainer()
        {
        }

        public JwtContainer(string jwt)
        {
            Jwt = jwt;
        }

        public string Jwt { get; set; }
    }
}