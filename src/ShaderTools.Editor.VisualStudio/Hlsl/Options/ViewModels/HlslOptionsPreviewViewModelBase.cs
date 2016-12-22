using System;
using Microsoft.VisualStudio.ComponentModelHost;
using ShaderTools.Core.Text;
using ShaderTools.Hlsl.Formatting;
using ShaderTools.Hlsl.Syntax;
using ShaderTools.Editor.VisualStudio.Core.Options.ViewModels;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Options.ViewModels
{
    internal abstract class HlslOptionsPreviewViewModelBase : OptionsPreviewViewModelBase
    {
        private readonly IHlslOptionsService _optionsService;

        protected HlslOptionsPreviewViewModelBase(IServiceProvider serviceProvider)
            : base(serviceProvider, HlslConstants.ContentTypeName)
        {
            var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
            _optionsService = componentModel.GetService<IHlslOptionsService>();
        }

        protected override string ApplyFormatting(string text)
        {
            var sourceText = SourceText.From(text);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText);
            var edits = Formatter.GetEdits(syntaxTree, new TextSpan(sourceText, 0, text.Length), _optionsService.FormattingOptions);
            return Formatter.ApplyEdits(text, edits);
        }
    }
}