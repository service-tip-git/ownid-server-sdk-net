namespace OwnIdSdk.NetCore3.Web.Extensibility
{
    public class LoginResult<T> where T : class
    {
        public T Data { get; set; }

        public int HttpCode { get; set; }
    }
}