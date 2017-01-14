using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;

namespace ShaderTools.Core.Options
{
    public static class ConfigFileLoader
    {
        private static readonly DataContractJsonSerializer Serializer = new DataContractJsonSerializer(typeof(ConfigFile), new DataContractJsonSerializerSettings()
        {
            UseSimpleDictionaryFormat = true
        });

        public static ConfigFile LoadAndMergeConfigFile(string filePath)
        {
            if (filePath == null)
                return new ConfigFile();

            var initialDirectory = Path.GetDirectoryName(filePath);
            var configFiles = GetConfigFiles(initialDirectory);

            // We want closer config files to take precedence over further ones.

            var hlslPreprocessorDefinitions = new Dictionary<string, string>();
            foreach (var configFile in configFiles.Reverse())
                foreach (var preprocessorDefinition in configFile.HlslPreprocessorDefinitions)
                    hlslPreprocessorDefinitions[preprocessorDefinition.Key] = preprocessorDefinition.Value;

            return new ConfigFile
            {
                HlslPreprocessorDefinitions = hlslPreprocessorDefinitions,

                HlslAdditionalIncludeDirectories = configFiles
                    .SelectMany(x => x.GetAbsoluteHlslAdditionalIncludeDirectories())
                    .Distinct()
                    .ToList()
            };
        }

        private static IEnumerable<ConfigFile> GetConfigFiles(string initialDirectory)
        {
            var directoryInfo = new DirectoryInfo(initialDirectory);

            var result = new List<ConfigFile>();
            while (true)
            {
                var configFilePath = Path.Combine(directoryInfo.FullName, "shadertoolsconfig.json");
                if (File.Exists(configFilePath))
                {
                    // TODO: Error handling.
                    ConfigFile configFile;
                    using (var configFileStream = File.OpenRead(configFilePath))
                        configFile = (ConfigFile) Serializer.ReadObject(configFileStream);

                    configFile.FileName = configFilePath;
                    result.Add(configFile);

                    if (configFile.Root)
                        break;
                }

                directoryInfo = directoryInfo.Parent;
                if (directoryInfo == null)
                    break;
            }
            return result;
        }
    }
}