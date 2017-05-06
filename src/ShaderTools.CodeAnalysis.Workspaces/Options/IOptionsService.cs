using System;
using ShaderTools.CodeAnalysis.Host;

namespace ShaderTools.CodeAnalysis.Options
{
    // TODO: This should be a workspace service, not a language service.
    internal interface IOptionsService : ILanguageService
    {
        event EventHandler OptionsChanged;

        void RaiseOptionsChanged();

        bool EnableIntelliSense { get; }
        bool EnableErrorReporting { get; }
        bool EnableSquiggles { get; }
    }
}
