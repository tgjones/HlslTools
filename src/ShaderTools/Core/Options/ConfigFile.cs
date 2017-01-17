using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;

namespace ShaderTools.Core.Options
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