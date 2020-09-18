using System.Collections.Generic;
using System.Linq;
using OwnIdSdk.NetCore3.Extensibility.Services;

namespace OwnIdSdk.NetCore3.Extensibility.Flow.Abstractions
{
    public interface IFormContext
    {
        /// <summary>
        ///     General, non-fields-specific errors
        /// </summary>
        public List<string> GeneralErrors { get; set; }

        /// <summary>
        ///     Fields-specific errors. Property name used as key
        /// </summary>
        public IReadOnlyDictionary<string, IList<string>> FieldErrors { get; }

        /// <summary>
        ///     Indicates if there is any error (general or field)
        /// </summary>
        bool HasErrors => GeneralErrors.Any() ||
                          FieldErrors.Any(x => x.Value.Any());

        /// <summary>
        ///     Executes validation with current context
        /// </summary>
        void Validate();

        /// <summary>
        ///     Adds general errors
        /// </summary>
        /// <param name="error">Error text that will be localized with <see cref="ILocalizationService" /></param>
        void SetGeneralError(string error);
    }
}