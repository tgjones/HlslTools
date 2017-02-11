using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using ShaderTools.Editor.VisualStudio.Core.Parsing;
using ShaderTools.Editor.VisualStudio.ShaderLab.Util.Extensions;

namespace ShaderTools.Editor.VisualStudio.ShaderLab.Parsing
{
    //[Export(typeof(IWpfTextViewCreationListener))]
    [ContentType(ShaderLabConstants.ContentTypeName)]
    [TextViewRole(PredefinedTextViewRoles.Interactive)]
    internal sealed class BackgroundParserManager : BackgroundParserManagerBase
    {
        protected override BackgroundParserBase GetBackgroundParser(ITextBuffer textBuffer)
        {
            return textBuffer.GetBackgroundParser();
        }
    }
}