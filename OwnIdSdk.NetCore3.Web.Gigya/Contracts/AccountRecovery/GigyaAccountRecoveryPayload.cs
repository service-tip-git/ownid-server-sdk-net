using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Web.Gigya.Contracts.AccountRecovery
{
    /// <summary>
    /// Gigya specific account recovery payload
    /// </summary>
    public class GigyaAccountRecoveryPayload
    {
        /// <summary>
        /// Contains Reset Token which allows to reset user password.
        /// </summary>
        /// <remarks>
        /// https://developers.gigya.com/display/GD/accounts.resetPassword+REST
        /// </remarks>
        [JsonPropertyName("pwrt")]
        public string ResetToken { get; set; }
    }
}