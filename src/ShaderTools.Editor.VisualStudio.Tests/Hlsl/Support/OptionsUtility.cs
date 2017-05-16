using System;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Hlsl.Options;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.Editor.VisualStudio.Tests.Hlsl.Support
{
    internal class FakeOptionsService : IHlslOptionsService
    {
        public FormattingOptions GetPrimaryWorkspaceFormattingOptions()
        {
            return new FormattingOptions();
        }

        public FormattingOptions GetFormattingOptions(OptionSet options)
        {
            return new FormattingOptions();
        }
    }
}