using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OwnIdSdk.NetCore3.Configuration.Profile
{
    /// <summary>
    ///     Interface to define Profile Configuration management mechanism
    /// </summary>
    public interface IProfileConfiguration
    {
        /// <summary>
        ///     User Profile type
        /// </summary>
        /// <remarks>
        ///     Used for describing User Profile model, validation and display rules
        /// </remarks>
        Type ProfileModelType { get; }

        /// <summary>
        ///     For Internal usage. Stores User Profile model, validation and display rules in proper way to easily pass it to
        ///     OwnId application
        /// </summary>
        IReadOnlyList<ProfileFieldMetadata> ProfileFieldMetadata { get; }

        /// <summary>
        ///     Indicates if <see cref="ProfileModelType" /> is correctly configured for usage
        /// </summary>
        /// <returns>Validation result of <see cref="ProfileModelType" /></returns>
        ValidateOptionsResult Validate();

        /// <summary>
        ///     Generates data for <see cref="ProfileFieldMetadata" />
        /// </summary>
        void BuildMetadata();
    }
}