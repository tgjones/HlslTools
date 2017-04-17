using System.Collections.Generic;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Workspace.Host.Mef
{
    /// <summary>
    /// MEF metadata class used for finding <see cref="ILanguageService"/> and <see cref="ILanguageServiceFactory"/> exports.
    /// </summary>
    internal class LanguageServiceMetadata : LanguageMetadata
    {
        public string ServiceType { get; }

        public IReadOnlyDictionary<string, object> Data { get; }

        public LanguageServiceMetadata(IDictionary<string, object> data)
            : base(data)
        {
            this.ServiceType = (string) data.GetValueOrDefault("ServiceType");
            this.Data = (IReadOnlyDictionary<string, object>) data;
        }
    }
}
