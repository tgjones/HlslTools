using System.Composition;
using ShaderTools.CodeAnalysis.Editor.Implementation.CommentSelection;
using ShaderTools.CodeAnalysis.Host.Mef;

namespace ShaderTools.CodeAnalysis.Editor.Hlsl.CommentSelection
{
    [ExportLanguageService(typeof(ICommentUncommentService), LanguageNames.Hlsl), Shared]
    internal class HlslCommentUncommentService : AbstractCommentUncommentService
    {
        public override string SingleLineCommentString => "//";

        public override bool SupportsBlockComment => true;

        public override string BlockCommentStartString => "/*";

        public override string BlockCommentEndString => "*/";
    }
}
