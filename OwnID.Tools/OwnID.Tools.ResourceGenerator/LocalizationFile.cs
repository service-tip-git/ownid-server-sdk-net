using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OwnID.Tools.ResourceGenerator
{
    public class LocalizationFile
    {
        private static readonly Regex Regex = new Regex(@"(?<name>[^_]*)_?(?<lang>.*)\.json", RegexOptions.Compiled);

        private static readonly Lazy<StringBuilder> ResxTemplate = new Lazy<StringBuilder>(() =>
        {
            var result = new StringBuilder();
            result.Append(Templates.ResxTemplate);
            return result;
        });

        private FileInfo File { get; set; }
        public string Name { get; set; }
        public string Language { get; set; }
        public string ResourceName { get; set; }

        /// <summary>
        ///     Indicate if new resource has been generated
        /// </summary>
        public bool IsNewFile { get; private set; }


        public Dictionary<string, string> Translations { get; set; } = new Dictionary<string, string>();

        public static async Task GenerateResxAsync(string projectFolder, FileInfo fileInfo,
            CancellationToken cancellationToken = default)
        {
            var inputLocalizationFile = new LocalizationFile();

            await inputLocalizationFile.LoadAsync(fileInfo, cancellationToken);

            // inputLocalizationFile.BuildBinaryResource();
            var resxFileName = await inputLocalizationFile.BuildResxAsync(cancellationToken);
            var designerFileName = inputLocalizationFile.BuildResxDesigner();

            await inputLocalizationFile.AddResourceToProject(projectFolder, resxFileName, designerFileName, cancellationToken);
        }

        private async Task AddResourceToProject(
            string projectFolder
            , FileInfo resxFile
            , FileInfo designerFile
            , CancellationToken cancellationToken)
        {
            if (!IsNewFile) 
                return;

            var di = new DirectoryInfo(projectFolder);
            var projectFile = di.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly).FirstOrDefault();
            if (projectFile == null)
                return;
            
            var readStream = new FileStream(projectFile.FullName, FileMode.Open, FileAccess.Read);
            var xmlDocument = await XDocument.LoadAsync(readStream, LoadOptions.None, cancellationToken);

            var itemGroup = new XElement("ItemGroup");

            var resourceDesignerElement = new XElement(
                "Compile"
                , new XAttribute("Update", GetRelativePath(projectFolder, designerFile))
                , new XElement("DesignTime", "True")
                , new XElement("AutoGen", "True")
                , new XElement("DependentUpon", resxFile.Name)
            );
            itemGroup.Add(resourceDesignerElement);
            
            var resxElement = new XElement(
                "EmbeddedResource"
                , new XAttribute("Update", GetRelativePath(projectFolder, resxFile))
                , new XElement("Generator", "ResXFileCodeGenerator")
                , new XElement("LastGenOutput", designerFile.Name)
            );
            itemGroup.Add(resxElement);

            xmlDocument.Root.Add(itemGroup);
            
            // save results to the file
            await using var writeStream = new FileStream(projectFile.FullName, FileMode.Create, FileAccess.Write);
            await xmlDocument.SaveAsync(writeStream, SaveOptions.None, cancellationToken);

            writeStream.Close();
        }

        private static string GetRelativePath(string projectFolder, FileInfo file)
        {
            var result = file.FullName.Replace(projectFolder, string.Empty);
            if (result.StartsWith(Path.DirectorySeparatorChar))
                result = result.Substring(1);
            result =
                result.Replace("/", @"\");
            return result;
        }


        private async Task LoadAsync(FileInfo file, CancellationToken cancellationToken = default)
        {
            var regexMatch = Regex.Match(file.Name);
            if (!regexMatch.Success)
                return;

            File = file;
            Name = regexMatch.Groups["name"].Value;
            Language = regexMatch.Groups["lang"].Value;

            var json = await JsonDocument.ParseAsync(file.OpenRead(), cancellationToken: cancellationToken);
            foreach (var topLevelElements in json.RootElement.EnumerateObject())
            {
                // The assumption is that we have only one top level element
                ResourceName = topLevelElements.Name;

                foreach (var translation in topLevelElements.Value.EnumerateObject())
                {
                    if (Translations.ContainsKey(translation.Name))
                    {
                        throw new Exception(
                            $"File '{file.FullName}' contains more than one element with key '{translation.Name}'");
                    }

                    Translations.Add(translation.Name, translation.Value.ToString());
                }
            }
        }

        private async Task<FileInfo> BuildResxAsync(CancellationToken cancellationToken = default)
        {
            var extension = string.IsNullOrEmpty(Language) ? "resx" : $"{Language}.resx";
            var path = Path.Combine(File.Directory.FullName, $"{ResourceName}.{extension}");

            await using var readStream = await OpenOrCreateResxStreamAsync(path, cancellationToken);
            var xmlDocument = await XDocument.LoadAsync(readStream, LoadOptions.None, cancellationToken);

            if (xmlDocument.Root == null)
                throw new Exception($"No root element at document (`{File.FullName}`)");

            // remove old translations
            foreach (var translationElement in xmlDocument.Root.Elements("data").ToArray())
            {
                translationElement.Remove();
            }

            // add translations
            foreach (var (key, value) in Translations)
            {
                var newTranslationElement = new XElement("data"
                    , new XAttribute("name", key)
                    , new XAttribute(XNamespace.Xml + "space", "preserve")
                );
                
                newTranslationElement.Add(new XElement("value", value));

                xmlDocument.Root.Add(newTranslationElement);
            }

            readStream.Close();

            // save results to the file
            await using var writeStream = new FileStream(path, FileMode.Create, FileAccess.Write);
            await xmlDocument.SaveAsync(writeStream, SaveOptions.None, cancellationToken);

            writeStream.Close();

            Console.WriteLine($"Resource file generated: {path}");

            return new FileInfo(path);
        }

        private async Task<FileStream> OpenOrCreateResxStreamAsync(string path,
            CancellationToken cancellationToken = default)
        {
            var resxFile = new FileInfo(path);
            if (!resxFile.Exists)
            {
                await using var writeStream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite);
                await using var streamWriter = new StreamWriter(writeStream);

                await streamWriter.WriteAsync(ResxTemplate.Value, cancellationToken);

                await streamWriter.FlushAsync();
                streamWriter.Close();
                writeStream.Close();

                IsNewFile = true;
            }

            // create new resx file from template
            // Return read stream if file already exists
            return new FileStream(path, FileMode.Open, FileAccess.Read);
        }


        private FileInfo BuildResxDesigner()
        {
            var extension = string.IsNullOrEmpty(Language) ? "Designer.cs" : $"{Language}.Designer.cs";
            var path = Path.Combine(File.Directory.FullName, $"{ResourceName}.{extension}");
            
            // check if language specified
            if (string.IsNullOrWhiteSpace(Language))
            {
                Console.WriteLine($"You need to create designer file manually (`{path}`)");
                return null;
            }

            // check if file already exists
            var designerFile = new FileInfo(path);
            if (designerFile.Exists)
                return null;

            using var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            fileStream.Close();

            return designerFile;
        }

        private void BuildBinaryResource()
        {
            if (File.Directory == null) return;

            var extension = String.IsNullOrEmpty(Language) ? "resources" : $"{Language}.resources";
            var path = Path.Combine(File.Directory.FullName, $"{Name}.{extension}");

            using var resx = new ResourceWriter(path);

            foreach (var (key, value) in Translations)
            {
                resx.AddResource(key, value);
            }

            resx.Generate();

            resx.Close();
            Console.WriteLine($"Binary resource file generated: {path}");
        }
    }
}