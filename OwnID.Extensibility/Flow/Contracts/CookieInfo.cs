using Microsoft.AspNetCore.Http;

namespace OwnID.Extensibility.Flow.Contracts
{
    public class CookieInfo
    {
        public string Name { get; set; }
        
        public string Value { get; set; }
        
        public CookieOptions Options { get; set; }
    }
}