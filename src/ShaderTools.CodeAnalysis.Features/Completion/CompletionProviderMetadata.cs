using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.CodeAnalysis.Completion.Providers
{
    internal sealed class CompletionProviderMetadata : OrderableLanguageMetadata
    {
        public string[] Roles { get; }

        public CompletionProviderMetadata(IDictionary<string, object> data)
            : base(data)
        {
            this.Roles = (string[]) data.GetValueOrDefault("Roles")
                         ?? (string[]) data.GetValueOrDefault("TextViewRoles");
        }
    }
}
