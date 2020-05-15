using System.ComponentModel.DataAnnotations;

namespace OwnIdSdk.NetCore3.Contracts
{
    public class LoginResult<T> where T: class
    {
        public T Data { get; set; }
        
        public int HttpCode { get; set; }
    }
}