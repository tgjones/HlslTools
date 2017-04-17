using System.Collections.Generic;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Workspace.Host.Mef
{
    /// <summary>
    /// MEF metadata class used to find exports declared for a specific language.
    /// </summary>
    internal class LanguageMetadata : ILanguageMetadata
    {
        public string Language { get; }

        public LanguageMetadata(IDictionary<string, object> data)
        {
            this.Language = (string) data.GetValueOrDefault("Language");
        }

        public LanguageMetadata(string language)
        {
            this.Language = language;
        }
    }
}
