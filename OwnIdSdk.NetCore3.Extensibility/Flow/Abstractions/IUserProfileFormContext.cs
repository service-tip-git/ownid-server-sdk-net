using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
    /// <summary>
    ///     Describes basic context mechanism with User Profile value and validation
    /// </summary>
    /// <typeparam name="TProfile">User Profile</typeparam>
    /// <inheritdoc cref="IFormContext" />
    public interface IUserProfileFormContext<TProfile> : IFormContext where TProfile : class
    {
        /// <summary>
        ///     User unique identifier
        /// </summary>
        string DID { get; }

        /// <summary>
        ///     User public key generated for current organization / website
        /// </summary>
        string PublicKey { get; }

        /// <summary>
        ///     User filled profile
        /// </summary>
        TProfile Profile { get; }

        /// <summary>
        ///     Set field-specific validation error
        /// </summary>
        /// <param name="exp">Property selector</param>
        /// <param name="errorText">Error text that will be localized with <see cref="ILocalizationService" /></param>
        /// <typeparam name="TField"><typeparamref name="TProfile" /> property</typeparam>
        void SetError<TField>([NotNull] Expression<Func<TProfile, TField>> exp, string errorText);
    }
}