﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace OwnID.Tools.Configurator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteAscii("OwnID", Color.DarkSlateGray);
            Console.WriteLine("OwnID Net Core SDK Configurator v.1", Color.DarkSlateGray);
            Console.WriteLine("This utility should be placed in the same directory application executables are for correct functioning.", Color.Goldenrod);
            Console.WriteLine("This utility will walk you through modifying a appsettings.json file.");
            Console.WriteLine("It only covers the most common settings. For more information please visit https://github.wdf.sap.corp/OwnID/server-sdk-net/wiki/advanced-configuration \n");
            Console.WriteLine($"Press ^C at any time to quit.", Color.Goldenrod);
            
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location);
            var defaultName = Path.GetFileName(currentDir);
            var keysPath = Path.Combine(currentDir, "keys");
            
            var createEntireConfig =  ReadValue("Create entire config (y) or RSA keys only (n)", "y", possibleValues: new []{"y", "n"});

            if (createEntireConfig == "n")
            {
                await GenerateKeysAsync(keysPath);
                Console.WriteLine($"Keys directory: {keysPath}", Color.Green);
                Console.WriteLine("Done! Press any key to close", Color.Green);
                Console.ReadKey();
                return;
            }
            
            var name = ReadValue("Website name", defaultName);
            var description = ReadValue("Description");
            var icon = ReadValue("Icon (url or base64)", isRequired: true);
            var callbackUrl = ReadValue("Callback Url (net core SDK address)", "https://localhost:5002/ownid");
            var overwriteFields = ReadValue("Overwrite fields", "n", possibleValues: new []{"y", "n"});

            var enableFido2 = ReadValue("Enable FIDO2?", "y", possibleValues: new[] {"y", "n"}) == "y";
            string fido2passwordlessPageUrl = null,
                fido2Origin = null,
                fido2RelyingPartyId = null,
                fido2RelyingPartyName = null,
                fido2UserDisplayName = null,
                fido2UserName = null;
            
            if (enableFido2)
            {
                fido2passwordlessPageUrl = ReadValue("FIDO2 -> Passwordless Page Url", isRequired: true);
                fido2Origin = ReadValue("FIDO2 -> Origin", isRequired: true);
                fido2RelyingPartyId = ReadValue("FIDO2 -> Relying Party Id");
                fido2RelyingPartyName = ReadValue("FIDO2 -> Relying Party Name");
                fido2UserDisplayName = ReadValue("FIDO2 -> User Display Name");
                fido2UserName = ReadValue("FIDO2 -> User Name");
            }
            
            var gigyaDataCenter = ReadValue("GIGYA -> Data Center", "us1.gigya.com");
            var gigyaApiKey = ReadValue("GIGYA -> Api Key", isRequired: true);
            var gigyaSecret = ReadValue("GIGYA -> Secret");
            var gigyaUserKey = ReadValue("GIGYA -> User Key");
            var loginType = ReadValue("GIGYA -> Login Type", "session",
                possibleValues: new[] {"session", "idtoken"});
            
            
            var redisConnection = ReadValue("Redis connection string");
            
            Console.WriteLine($"{Environment.NewLine}Processing...{Environment.NewLine}", Color.Blue);

            var keys = await GenerateKeysAsync(keysPath);
            
            var config = new Config
            {
                Name = name,
                Description = description,
                CallbackUrl = callbackUrl,
                Icon = icon,
                OverwriteFields = overwriteFields.ToLowerInvariant() == "y",
                PrivateKey = keys.privatePath,
                PublicKey = keys.publicPath,
                RedisConnection = redisConnection,
                DID = $"did:{name}:{Guid.NewGuid():N}",
                Fido2Enabled = enableFido2,
                Fido2PasswordlessPageUrl = fido2passwordlessPageUrl,
                Fido2Origin = fido2Origin,
                Fido2UserName = fido2UserName,
                Fido2RelyingPartyId = fido2RelyingPartyId,
                Fido2RelyingPartyName = fido2RelyingPartyName,
                Fido2UserDisplayName = fido2UserDisplayName
            };
            
            var configFilePath = Path.Combine(currentDir, "appsettings.json");
            
            var gigyaConfig = new GigyaConfig
            {
                Secret = gigyaSecret,
                ApiKey = gigyaApiKey,
                DataCenter = gigyaDataCenter,
                LoginType = loginType,
                UserKey = gigyaUserKey
            };

            var jsonConfig = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            if (File.Exists(configFilePath))
            {
                Console.WriteLine("Modifying config file...", Color.Gray);
                await using var appConfigStream = File.Open(configFilePath, FileMode.Open);
                var currentData = await JsonSerializer.DeserializeAsync<JsonElement>(appConfigStream);
                var a = currentData.EnumerateObject();

                var newConf = new Dictionary<string, object>();

                foreach (var sub in a)
                {
                    if(sub.Name != "ownid" && sub.Name != "gigya")
                        newConf.Add(sub.Name, sub.Value);
                }
                
                newConf.Add("ownid", config);
                newConf.Add("gigya", gigyaConfig);

                appConfigStream.SetLength(0);
                await JsonSerializer.SerializeAsync(appConfigStream, newConf, jsonConfig);
            }
            else
            {
                Console.WriteLine("Creating config file...", Color.Gray);
                await using var stream = File.Create(configFilePath);
                await JsonSerializer.SerializeAsync(stream, new
                {
                    ownid = config,
                    gigya = gigyaConfig
                }, jsonConfig);
            }

            Console.WriteLine();
            Console.WriteLine($"Config file: {configFilePath}", Color.Green);
            Console.WriteLine($"Keys directory: {keysPath}", Color.Green);
            Console.WriteLine("Done! Press any key to close", Color.Green);
            Console.ReadKey();
        }

        static async Task<(string publicPath, string privatePath)> GenerateKeysAsync(string path)
        {
            Console.WriteLine($"Creating keys...", Color.Gray);
            
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            
            var keys = RSA.Create(4096);
            var privateKey = $"-----BEGIN RSA PRIVATE KEY-----{Environment.NewLine}{Convert.ToBase64String(keys.ExportRSAPrivateKey())}{Environment.NewLine}-----END RSA PRIVATE KEY-----";
            var publicKey = $"-----BEGIN RSA PUBLIC KEY-----{Environment.NewLine}{Convert.ToBase64String(keys.ExportRSAPublicKey())}{Environment.NewLine}-----END RSA PUBLIC KEY-----";

            var publicPath = Path.Combine(path, "key.pub");
            var privatePath = Path.Combine(path, "key.private");

            File.Delete(privatePath);
            File.Delete(publicPath);
            
            await using var pubFile = File.Open(publicPath, FileMode.CreateNew);
            await using var privateFile = File.Open(privatePath, FileMode.CreateNew);
            await pubFile.WriteAsync(System.Text.Encoding.UTF8.GetBytes(publicKey));
            await privateFile.WriteAsync(System.Text.Encoding.UTF8.GetBytes(privateKey));
            await pubFile.FlushAsync();
            await privateFile.FlushAsync();

            return (publicPath, privatePath);
        }

        static string ReadValue(string name, string defaultValue = null, bool isRequired = false, string[] possibleValues = null)
        {
            while (true)
            {
                var defaultSection = defaultValue != null ? $" ({defaultValue})" : string.Empty;
                var possibleValuesSection = possibleValues == null ? string.Empty : $" ({string.Join(", ", possibleValues)})";
                Console.Write($"{name}{possibleValuesSection}:{defaultSection} ");
                var value = Console.ReadLine();
                var isEmpty = string.IsNullOrWhiteSpace(value);
                
                if (isEmpty && isRequired)
                {
                    Console.WriteLine("Not empty or white spaced value expected\n", Color.OrangeRed);
                    continue;
                }

                if (isEmpty && defaultValue != null)
                    return defaultValue;
                
                if (possibleValues != null && (value == null || possibleValues.All(x=>x != value?.ToLowerInvariant())))
                {
                    Console.WriteLine($"Please select one of the values: {string.Join(", ", possibleValues)}\n", Color.OrangeRed);
                    continue;
                }

                return possibleValues == null ? value : value.ToLowerInvariant();
            }
        }
    }
}