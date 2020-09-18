using System;
using System.Diagnostics;
using System.Globalization;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;

namespace OwnIdSdk.NetCore3.Flow.Commands
{
    public interface ICommandInput
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public DateTime ClientDate { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Context) + "}")]
    public class CommandInput : ICommandInput
    {
        public CommandInput(RequestIdentity requestIdentity, CultureInfo cultureInfo, DateTime clientDate)
        {
            Context = requestIdentity.Context;
            RequestToken = requestIdentity.RequestToken;
            ResponseToken = requestIdentity.ResponseToken;
            CultureInfo = cultureInfo;
            ClientDate = clientDate;
        }

        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public DateTime ClientDate { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Context) + "}: {" + nameof(Data) + "}")]
    public class CommandInput<T> : CommandInput
    {
        public CommandInput(RequestIdentity requestIdentity, CultureInfo cultureInfo, T data, DateTime clientDate) :
            base(requestIdentity, cultureInfo, clientDate)
        {
            Data = data;
        }

        public T Data { get; }
    }
}