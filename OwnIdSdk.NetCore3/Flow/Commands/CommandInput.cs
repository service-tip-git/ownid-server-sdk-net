using System.Globalization;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public class CommandInput<T> : ICommandInput
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }

        public T Data { get; set; }

        public CultureInfo CultureInfo { get; set; }
    }
    
    public class CommandInput : ICommandInput
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }

        public CultureInfo CultureInfo { get; set; }
    }

    public interface ICommandInput
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }
        
        public CultureInfo CultureInfo { get; set; }
    }
}