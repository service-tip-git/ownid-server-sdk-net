using Microsoft.AspNetCore.Http;

namespace OwnID.Extensibility.Flow.Contracts.Cookies
{
    public class CookieInfo
    {
        public string Name { get; set; }
        
        public string Value { get; set; }
        
        public bool Remove { get; set; }
        
        public CookieOptions Options { get; set; }
    }
}