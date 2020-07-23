using System;
using System.Security.Cryptography;
using OwnIdSdk.NetCore3.Configuration.Profile;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Configuration.Profile;

namespace OwnIdSdk.NetCore3.Configuration
{
    /// <summary>
    ///     Core configuration that required for basic functioning
    /// </summary>
    /// <remarks>
    ///     Implements <see cref="IDisposable" />
    /// </remarks>
    /// <inheritdoc cref="IOwnIdCoreConfiguration" />
    public class OwnIdCoreConfiguration : IOwnIdCoreConfiguration, IDisposable
    {
        /// <summary>
        ///     Creates instance with default parameters
        /// </summary>
        /// <remarks>
        ///     Calls <see cref="OwnIdCoreConfiguration(System.Type)" /> with <see cref="DefaultProfileModel" />
        /// </remarks>
        public OwnIdCoreConfiguration() : this(typeof(DefaultProfileModel))
        {
        }

        /// <summary>
        ///     Creates instance with provided Profile Type
        /// </summary>
        /// <param name="profileType">Value for <see cref="ProfileConfiguration" /> instantiation</param>
        public OwnIdCoreConfiguration(Type profileType)
        {
            OwnIdApplicationUrl = new Uri(Constants.OwinIdApplicationAddress);
            ProfileConfiguration = new ProfileConfiguration(profileType);
        }

        // public string RegisterInstructions { get; set; }
        //
        // public string LoginInstructions { get; set; }

        /// <summary>
        ///     Disposes <see cref="JwtSignCredentials" />
        /// </summary>
        public void Dispose()
        {
            JwtSignCredentials?.Dispose();
        }

        public Uri OwnIdApplicationUrl { get; set; }

        public Uri CallbackUrl { get; set; }

        public RSA JwtSignCredentials { get; set; }

        public IProfileConfiguration ProfileConfiguration { get; private set; }

        public string DID { get; set; }

        public string Name { get; set; }

        public string Icon { get; set; }

        public string Description { get; set; }

        public int PollingInterval { get; set; }

        public bool IsDevEnvironment { get; set; }

        public uint CacheExpirationTimeout { get; set; }

        public uint MaximumNumberOfConnectedDevices { get; set; }

        public uint JwtExpirationTimeout { get; set; }

        /// <summary>
        ///     Sets <see cref="ProfileConfiguration" /> based on <typeparamref name="TProfile" />
        /// </summary>
        /// <typeparam name="TProfile">User Profile mode type</typeparam>
        public void SetProfileModel<TProfile>() where TProfile : class
        {
            ProfileConfiguration = new ProfileConfiguration(typeof(TProfile));
        }
    }
}