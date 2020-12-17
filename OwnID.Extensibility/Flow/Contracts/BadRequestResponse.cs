using System.Collections.Generic;
using OwnID.Extensibility.Configuration.Profile;

namespace OwnID.Extensibility.Flow.Contracts
{
    /// <summary>
    ///     Describes 400 BadRequest response for OwnId application
    /// </summary>
    public class BadRequestResponse
    {
        /// <summary>
        ///     General, non-field-specific localized error texts
        /// </summary>
        public IEnumerable<string> GeneralErrors { get; set; }

        /// <summary>
        ///     Field-specific localized error texts
        /// </summary>
        /// <remarks>
        ///     Key should contain <see cref="ProfileFieldMetadata.Key" />
        ///     Value should contain already localized error texts with localized field label inside if needed
        /// </remarks>
        public IDictionary<string, IList<string>> FieldErrors { get; set; }
    }
}