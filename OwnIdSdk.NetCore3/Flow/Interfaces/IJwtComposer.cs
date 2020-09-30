using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.ConnectionRecovery;

namespace OwnIdSdk.NetCore3.Flow.Interfaces
{
    public interface IJwtComposer
    {
        /// <summary>
        ///     Creates JWT challenge with requested information by OwnId app
        /// </summary>
        /// <param name="info">Base composition info</param>
        /// <param name="did">User unique identifier</param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        string GenerateProfileConfigJwt(BaseJwtComposeInfo info, string did);

        /// <summary>
        ///     Generates JWT for pin step
        /// </summary>
        /// <param name="info">Base composition info</param>
        /// <param name="pin">PIN code generated by <see cref="SetSecurityCode" /></param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        string GeneratePinStepJwt(BaseJwtComposeInfo info, string pin);

        /// <summary>
        ///     Generates final step JWT
        /// </summary>
        /// <param name="info">Base composition info</param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        string GenerateFinalStepJwt(BaseJwtComposeInfo info);

        /// <summary>
        ///     Generates basic step info JWT
        /// </summary>
        /// <param name="info">Base composition info</param>
        /// <param name="did">User unique identifier</param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        string GenerateBaseStepJwt(BaseJwtComposeInfo info, string did);

        /// <summary>
        ///     Generates JWT for performing recovery process
        /// </summary>
        /// <param name="info">Base composition info</param>
        /// <param name="data">Recovery data</param>
        /// <returns>Base64 encoded string that contains JWT</returns>
        string GenerateRecoveryDataJwt(BaseJwtComposeInfo info, ConnectionRecoveryResult<object> data);
    }
}