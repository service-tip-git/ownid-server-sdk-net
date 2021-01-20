using System.Text.Json.Serialization;

namespace OwnID.Extensibility.Flow.Contracts
{
    public class AuthResult<T> : AuthResult where T : class
    {
        public AuthResult(T data) : this(null, data)
        {
        }

        public AuthResult(string errorMessage, T data = null) : base(errorMessage)
        {
            Data = data;
        }

        public T Data { get; set; }
    }
    
    public class AuthResult 
    {
        public AuthResult()
        {
        }
        
        public AuthResult(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        
        [JsonPropertyName("error")]
        public string ErrorMessage { get; set; }

        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);
    }
}