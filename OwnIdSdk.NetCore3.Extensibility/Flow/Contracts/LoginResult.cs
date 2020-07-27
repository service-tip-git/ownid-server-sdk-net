using System.Text.Json.Serialization;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Contracts
{
    public class LoginResult<T> where T : class
    {
        public LoginResult(T data) : this(null, data)
        {
        }

        public LoginResult(string errorMessage, T data = null)
        {
            ErrorMessage = errorMessage;
            Data = data;
        }

        public T Data { get; set; }

        [JsonPropertyName("error")]
        public string ErrorMessage { get; set; }
    }
}