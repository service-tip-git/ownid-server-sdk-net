using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts
{
    public class AuthResult<T> where T : class
    {
        public AuthResult(T data) : this(null, data)
        {
        }

        public AuthResult(string errorMessage, T data = null)
        {
            ErrorMessage = errorMessage;
            Data = data;
        }

        public T Data { get; set; }

        [JsonPropertyName("error")]
        public string ErrorMessage { get; set; }
    }
}