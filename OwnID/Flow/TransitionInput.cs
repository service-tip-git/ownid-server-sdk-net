using System;
using System.Diagnostics;
using System.Globalization;
using OwnID.Extensibility.Flow.Contracts;

namespace OwnID.Flow
{
    public interface ITransitionInput
    {
        public string Context { get; set; }

        public string RequestToken { get; set; }

        public string ResponseToken { get; set; }

        public CultureInfo CultureInfo { get; set; }

        public DateTime ClientDate { get; set; }
        
        public bool IsDesktop { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Context) + "}")]
    public class TransitionInput : ITransitionInput
    {
        public TransitionInput(RequestIdentity requestIdentity, CultureInfo cultureInfo, DateTime clientDate)
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
        public bool IsDesktop { get; set; }
    }

    [DebuggerDisplay("{" + nameof(Context) + "}: {" + nameof(Data) + "}")]
    public class TransitionInput<T> : TransitionInput
    {
        public TransitionInput(RequestIdentity requestIdentity, CultureInfo cultureInfo, T data, DateTime clientDate) :
            base(requestIdentity, cultureInfo, clientDate)
        {
            Data = data;
        }

        public T Data { get; }
    }
}