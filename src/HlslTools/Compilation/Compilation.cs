using HlslTools.Binding;
using HlslTools.Syntax;

namespace HlslTools.Compilation
{
    public sealed class Compilation
    {
        public Compilation(SyntaxTree syntaxTree)
        {
            SyntaxTree = syntaxTree;
        }

        public SyntaxTree SyntaxTree { get; }

        public SemanticModel GetSemanticModel()
        {
            var bindingResult = Binder.Bind(SyntaxTree.Root);
            return new SemanticModel(this, bindingResult);
        }
    }
}