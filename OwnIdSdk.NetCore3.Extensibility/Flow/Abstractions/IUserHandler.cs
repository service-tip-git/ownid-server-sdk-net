using System.Threading.Tasks;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Fido2;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
    /// <summary>
    ///     Describes base User handling operations
    /// </summary>
    /// <remarks>
    ///     Implement this interface to manually integrate into challenge process with custom User Profile model.
    ///     Use
    ///     <c>
    ///         IServiceCollection.AddOwnId(builder => { builder.UseUserHandlerWithCustomProfile<![CDATA[<MyHandler>]]>(); })
    ///     </c>
    ///     for this purpose
    /// </remarks>
    /// <typeparam name="TProfile">User Profile</typeparam>
    public interface IUserHandler<TProfile> where TProfile : class
    {
        /// <summary>
        ///     Will be called whenever a user provided his profile data on registration or login
        /// </summary>
        /// <param name="context">
        ///     <see cref="IUserProfileFormContext{TProfile}" /> that provides valid user information and allows
        ///     to add validation or general errors
        /// </param>
        Task CreateProfileAsync(IUserProfileFormContext<TProfile> context);
        
        /// <summary>
        ///     Will be called whenever a user provided his profile data on registration or login
        /// </summary>
        /// <param name="context">
        ///     <see cref="IUserProfileFormContext{TProfile}" /> that provides valid user information and allows
        ///     to add validation or general errors
        /// </param>
        Task UpdateProfileAsync(IUserProfileFormContext<TProfile> context);

        /// <summary>
        ///     Will be called whenever a user waits for authorization credentials on success login. Data passed to
        ///     <see cref="LoginResult{T}" /> will be sent to OwnId UI SDK and passed to provided in configuration callback
        /// </summary>
        /// <param name="did">User unique identifier</param>
        /// <param name="publicKey">User public key</param>
        Task<LoginResult<object>> OnSuccessLoginAsync(string did, string publicKey);

        /// <summary>
        ///     Will be called whenever a user waits for authorization credentials on success login. Data passed to
        ///     <see cref="LoginResult{T}" /> will be sent to OwnId UI SDK and passed to provided in configuration callback
        /// </summary>
        /// <param name="publicKey">User public key</param>
        Task<LoginResult<object>> OnSuccessLoginByPublicKeyAsync(string publicKey);

        /// <summary>
        ///     Will be called whenever a user waits for authorization credentials on success login. Data passed to
        ///     <see cref="LoginResult{T}" /> will be sent to OwnId UI SDK and passed to provided in configuration callback
        /// </summary>
        /// <param name="fido2UserId">fido2 user id</param>
        /// <param name="fido2SignCounter">fido2 sign counter</param>
        Task<LoginResult<object>> OnSuccessLoginByFido2Async(string fido2UserId, uint fido2SignCounter);
        
        /// <summary>
        /// Will be called to define if user with such did and public key exists.
        /// During this method following checks should be preformed: user exists, public key exists in user data. 
        /// </summary>
        /// <param name="did">User unique identifier</param>
        /// <param name="publicKey">User public key</param>
        /// <returns>Check result <see cref="IdentitiesCheckResult"/></returns>
        Task<IdentitiesCheckResult> CheckUserIdentitiesAsync(string did, string publicKey);
        
        /// <summary>
        ///     Try find Fido2 public key by fido2 user id
        /// </summary>
        /// <param name="fido2UserId">fido2 user id</param>
        /// <returns>
        ///     A task that represents the asynchronous find operation.
        ///     The task result contains fido2 information if user has been found,
        ///     otherwise null
        /// </returns>
        Task<Fido2Info> FindFido2Info(string fido2UserId);
    }
}