using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using Console = Colorful.Console;

namespace OwnIdSdk.Tools.Configurator
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
            
            var name = ReadValue("Website name", defaultName);
            var description = ReadValue("Description");
            var icon = ReadValue("Icon (url or base64)", isRequired: true);
            var callbackUrl = ReadValue("Callback Url (net core SDK address)", "https://localhost:5002/ownid");
            var overwriteFields = ReadValue("Overwrite fields", "n", possibleValues: new []{"y", "n"});
            
            var gigyaDataCenter = ReadValue("GIGYA - Data Center", "us1.gigya.com");
            var gigyaApiKey = ReadValue("GIGYA - Api Key", isRequired: true);
            var gigyaSecret = ReadValue("GIGYA - Secret");
            var gigyaUserKey = ReadValue("GIGYA - User Key");
            var loginType = ReadValue("GIGYA - login type", "session",
                possibleValues: new[] {"session", "idtoken"});
            
            
            var redisConnection = ReadValue("Redis connection string");
            
            Console.WriteLine($"{Environment.NewLine}Processing...{Environment.NewLine}", Color.Blue);

            Console.WriteLine($"Creating keys...", Color.Gray);
            var keysPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly()?.Location), "keys");

            if (!Directory.Exists(keysPath))
                Directory.CreateDirectory(keysPath);
            
            var keys = RSA.Create(4096);
            var privateKey = $"-----BEGIN RSA PRIVATE KEY-----{Environment.NewLine}{Convert.ToBase64String(keys.ExportRSAPrivateKey())}{Environment.NewLine}-----END RSA PRIVATE KEY-----";
            var publicKey = $"-----BEGIN RSA PUBLIC KEY-----{Environment.NewLine}{Convert.ToBase64String(keys.ExportRSAPublicKey())}{Environment.NewLine}-----END RSA PUBLIC KEY-----";
            
            var config = new Config
            {
                Name = name,
                Description = description,
                CallbackUrl = callbackUrl,
                Icon = icon,
                OverwriteFields = overwriteFields.ToLowerInvariant() == "y",
                PrivateKey = Path.Combine(keysPath, "key.private"),
                PublicKey = Path.Combine(keysPath, "key.pub"),
                RedisConnection = redisConnection,
                DID = $"did:{name}:{Guid.NewGuid():N}"
            };
            
            File.Delete(config.PrivateKey);
            File.Delete(config.PublicKey);

            await using var pubFile = File.Open(config.PublicKey, FileMode.CreateNew);
            await using var privateFile = File.Open(config.PrivateKey, FileMode.CreateNew);
            await pubFile.WriteAsync(System.Text.Encoding.UTF8.GetBytes(publicKey));
            await privateFile.WriteAsync(System.Text.Encoding.UTF8.GetBytes(privateKey));
            await pubFile.FlushAsync();
            await privateFile.FlushAsync();
            
            var configFilePath = Path.Combine(currentDir, "appsettings.json");
            
            var gigyaConfig = new GigyaConfig
            {
                Secret = gigyaSecret,
                ApiKey = gigyaApiKey,
                DataCenter = gigyaDataCenter,
                LoginType = loginType,
                UserKey = gigyaUserKey
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
                await JsonSerializer.SerializeAsync(appConfigStream, newConf, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    IgnoreNullValues = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });
            }
            else
            {
                Console.WriteLine("Creating config file...", Color.Gray);
                await using var stream = File.Create(configFilePath);
                await JsonSerializer.SerializeAsync(stream, new
                {
                    ownid = config,
                    gigya = gigyaConfig
                });
            }

            Console.WriteLine();
            Console.WriteLine($"Config file: {configFilePath}", Color.Green);
            Console.WriteLine($"Keys directory: {keysPath}", Color.Green);
            Console.WriteLine("Done! Press any key to close", Color.Green);
            Console.ReadKey();
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