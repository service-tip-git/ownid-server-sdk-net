using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using OwnIdSdk.NetCore3.Extensibility.Configuration;
using OwnIdSdk.NetCore3.Extensibility.Flow.Contracts.Jwt;

namespace OwnIdSdk.NetCore3.Cryptography
{
    public class JwtService : IJwtService
    {
        private readonly IOwnIdCoreConfiguration _ownIdCoreConfiguration;

        public JwtService(IOwnIdCoreConfiguration ownIdCoreConfiguration)
        {
            _ownIdCoreConfiguration = ownIdCoreConfiguration;
        }

        /// <summary>
        ///     Decodes provided by OwnId application JWT with data
        /// </summary>
        /// <param name="jwt">Base64 JWT string </param>
        /// <typeparam name="TData">Data type used as model for deserialization</typeparam>
        /// <returns>Context(challenge unique identifier) with <see cref="UserProfileData" /></returns>
        /// <exception cref="Exception">If something went wrong during token validation</exception>
        public (string Context, TData Data) GetDataFromJwt<TData>(string jwt) where TData : ISignedData
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(jwt)) throw new Exception("invalid jwt");

            var token = tokenHandler.ReadJwtToken(jwt);
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            var data = JsonSerializer.Deserialize<TData>(token.Payload["data"].ToString(), serializerOptions);
            // TODO: add type of challenge
            using var sr = new StringReader(data.PublicKey);
            var rsaSecurityKey = new RsaSecurityKey(RsaHelper.LoadKeys(sr));

            try
            {
                tokenHandler.ValidateToken(jwt, new TokenValidationParameters
                {
                    IssuerSigningKey = rsaSecurityKey,
                    RequireSignedTokens = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false
                    // TODO: add issuer to token for validation
                }, out _);
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new Exception($"Token failed validation: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Token was invalid: {ex.Message}");
            }

            return (token.Id, data);
        }

        public string GenerateDataJwt(Dictionary<string, object> data)
        {
            var rsaSecurityKey = new RsaSecurityKey(_ownIdCoreConfiguration.JwtSignCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();

            //TODO: should be received from the user's phone
            var issuedAt = DateTime.UtcNow.Add(TimeSpan.FromHours(-1));
            var notBefore = issuedAt;
            var expires = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(_ownIdCoreConfiguration.JwtExpirationTimeout));

            var payload = new JwtPayload(null, null, null, notBefore, expires, issuedAt);

            foreach (var (key, value) in data) payload.Add(key, value);

            var jwt = new JwtSecurityToken(
                new JwtHeader(new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)), payload);

            return tokenHandler.WriteToken(jwt);
        }


        /// <summary>
        ///     Gets hash of Base64 encoded JWT string
        /// </summary>
        /// <param name="jwt">Base64 encoded JWT string</param>
        /// <returns>SHA1 Base64 encoded string</returns>
        public string GetJwtHash(string jwt)
        {
            using var sha1 = new SHA1Managed();

            var b64 = Encoding.UTF8.GetBytes(jwt);
            var hash = sha1.ComputeHash(b64);

            return Convert.ToBase64String(hash);
        }
    }
}