using System.Collections.Generic;
using System.Collections.Immutable;
using ShaderTools.CodeAnalysis.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Parser;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Syntax
{
    public abstract partial class DirectiveTriviaSyntax : StructuredTriviaSyntax
    {
        internal override DirectiveStack ApplyDirectives(DirectiveStack stack)
        {
            return stack.Add(new Directive(this));
        }
    }

    public sealed partial class IfDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        public override ExpressionSyntax Condition => null;
    }

    public sealed partial class IfNDefDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
    {
        public override ExpressionSyntax Condition => null;
    }

    public sealed partial class IncludeDirectiveTriviaSyntax : DirectiveTriviaSyntax
    {
        public string TrimmedFilename => Filename.Text.TrimStart('<', '"').TrimEnd('>', '"');
        public bool IsAngled => Filename.Text.StartsWith("<");
    }
}