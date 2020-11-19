using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace OwnID.Tools.ResourceGenerator
{
    static class Program
    {
        private static IConfigurationRoot Configuration { get; set; }

        static void Main(string[] args)
        {
            InitConfiguration(args);
            
            if (!Directory.Exists(Configuration[CommandLineArgs.Source]))
                throw new ArgumentException($"Folder doesn't exist: `{Configuration[CommandLineArgs.Source]}`");

            var localizationFiles = FindLocalizationFiles(Configuration[CommandLineArgs.Source]);

            Task.WaitAll(
                localizationFiles.Select(localizationFile => LocalizationFile.GenerateResxAsync(
                    Configuration[CommandLineArgs.Source]
                    , localizationFile
                    , CancellationToken.None)).ToArray()
            );
        }


        /// <summary>
        /// Init configuration from command line
        /// </summary>
        /// <param name="args">command line arguments</param>
        /// <remarks>
        /// Uses https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.configuration.commandlineconfigurationextensions.addcommandline?view=dotnet-plat-ext-3.1
        /// </remarks>
        private static void InitConfiguration(string[] args)
        {
            var switchMappings = new Dictionary<string, string>
            {
                {"-s", CommandLineArgs.Source},
            };
            var builder = new ConfigurationBuilder();

            builder.AddCommandLine(args, switchMappings);

            Configuration = builder.Build();
        }

        private static IEnumerable<FileInfo> FindLocalizationFiles(string path)
        {
            if (!Directory.Exists(path))
                throw new ArgumentException("Path doesn't exists", CommandLineArgs.Source);

            var sourceDirectory = new DirectoryInfo(path);
            
            //
            // TODO: get search pattern from translations_v2.json file
            //
            return sourceDirectory.EnumerateFiles("*strings*.json", SearchOption.AllDirectories).ToList();
        }
    }
}