using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ShaderTools.CodeAnalysis.Options
{
    [DataContract]
    public sealed class ConfigFile
    {
        // Not stored in file.
        internal string FileName { get; set; }

        [DataMember(Name = "root")]
        public bool Root { get; set; } = false;

        [DataMember(Name = "hlsl.preprocessorDefinitions")]
        public Dictionary<string, string> HlslPreprocessorDefinitions { get; set; } = new Dictionary<string, string>();

        [DataMember(Name = "hlsl.additionalIncludeDirectories")]
        public List<string> HlslAdditionalIncludeDirectories { get; set; } = new List<string>();

        [DataMember(Name = "hlsl.virtualDirectoryMappings")]
        public Dictionary<string, string> HlslVirtualDirectoryMappings { get; set; } = new Dictionary<string, string>();

        [OnDeserialized]
        private void OnDeserialized(StreamingContext c)
        {
            if (HlslPreprocessorDefinitions == null)
                HlslPreprocessorDefinitions = new Dictionary<string, string>();

            if (HlslAdditionalIncludeDirectories == null)
                HlslAdditionalIncludeDirectories = new List<string>();

            if (HlslVirtualDirectoryMappings == null)
                HlslVirtualDirectoryMappings = new Dictionary<string, string>();
        }

        /// <summary>
        /// Converts the (potentially) relative include directory paths to absolute directory paths.
        /// </summary>
        internal IEnumerable<string> GetAbsoluteHlslAdditionalIncludeDirectories()
        {
            string folder = Path.GetDirectoryName(FileName);

            return HlslAdditionalIncludeDirectories
                .Select(x =>
                {
                    if (Path.IsPathRooted(x))
                        return x;
                    return Path.Combine(folder, x.Replace("/", "\\"));
                })
                .Select(x => Path.GetFullPath(x)); // Expand . and ..
        }
    }
}