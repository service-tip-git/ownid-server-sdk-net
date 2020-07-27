using System;
using System.Collections.Generic;
using System.Text.Json;
using OwnIdSdk.NetCore3.Extensibility.Json;

namespace OwnIdSdk.NetCore3.Web.Gigya.ApiClient
{
    internal static class ParametersFactory
    {
        /// <summary>
        ///     Create initial parameters collection with populated auth parameters
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>parameters collection</returns>
        public static IList<KeyValuePair<string, string>> CreateAuthParameters(GigyaConfiguration configuration)
        {
            var result = CreateApiKeyParameter(configuration)
                .AddParameter("secret", configuration.SecretKey);

            if (!string.IsNullOrEmpty(configuration.UserKey))
                result.AddParameter("userKey", configuration.UserKey);

            return result;
        }

        /// <summary>
        ///     Create initial parameters collection with populated apiKey parameter
        /// </summary>
        /// <param name="configuration">configuration</param>
        /// <returns>parameters collection</returns>
        public static IList<KeyValuePair<string, string>> CreateApiKeyParameter(GigyaConfiguration configuration)
        {
            var result = new List<KeyValuePair<string, string>>();

            result.AddParameter("apiKey", configuration.ApiKey);

            return result;
        }

        public static IList<KeyValuePair<string, string>> AddParameter(
            this IList<KeyValuePair<string, string>> nameValueCollection
            , string key
            , string value)
        {
            if (!string.IsNullOrEmpty(value))
                nameValueCollection.Add(new KeyValuePair<string, string>(key, value));

            return nameValueCollection;
        }

        public static IList<KeyValuePair<string, string>> AddParameter<T>(
            this IList<KeyValuePair<string, string>> nameValueCollection
            , string key
            , T value)
        {
            nameValueCollection.Add(
                new KeyValuePair<string, string>(key, OwnIdSerializer.Serialize(value)));

            return nameValueCollection;
        }
    }
}