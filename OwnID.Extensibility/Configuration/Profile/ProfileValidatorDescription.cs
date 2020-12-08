using System.Diagnostics.CodeAnalysis;

namespace OwnID.Extensibility.Configuration.Profile
{
    /// <summary>
    ///     Defines OwnId app validator type and localized message with placeholders
    /// </summary>
    public class ProfileValidatorDescription
    {
        /// <summary>
        ///     Creates instance of <see cref="ProfileValidatorDescription" /> with provided parameters
        /// </summary>
        /// <param name="clientSideTypeNaming">Value for <see cref="ClientSideTypeNaming" />. OwnId application validator type</param>
        /// <param name="defaultErrorMessage">
        ///     Value for <see cref="DefaultErrorMessage" />. Localized error message with placeholders
        /// </param>
        public ProfileValidatorDescription([NotNull] string clientSideTypeNaming, [NotNull] string defaultErrorMessage)
        {
            ClientSideTypeNaming = clientSideTypeNaming;
            DefaultErrorMessage = defaultErrorMessage;
        }

        /// <summary>
        ///     OwnId application validator type
        /// </summary>
        public string ClientSideTypeNaming { get; }

        /// <summary>
        ///     Localized error message with placeholders
        /// </summary>
        public string DefaultErrorMessage { get; }
    }
}