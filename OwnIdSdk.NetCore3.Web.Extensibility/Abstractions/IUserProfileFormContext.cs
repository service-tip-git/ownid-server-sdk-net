using System;
using System.Linq.Expressions;

namespace OwnIdSdk.NetCore3.Web.Extensibility.Abstractions
{
    public interface IUserProfileFormContext<TProfile> : IFormContext where TProfile : class
    {
        string DID { get; }

        string PublicKey { get; }

        TProfile Profile { get; }

        void SetError<TField>(Expression<Func<TProfile, TField>> exp, string errorText);
    }
}