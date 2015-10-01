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
            //var bindingResult = new BindingResult(SyntaxTree.Root);
            //SymbolBinder.Bind(SyntaxTree.Root, bindingResult);
            return new SemanticModel(this);
        }
    }
}