using System;
using System.Threading;
using System.Threading.Tasks;
using ShaderTools.Core.Syntax;
using ShaderTools.Core.Text;
using ShaderTools.EditorServices.Utility;

namespace ShaderTools.EditorServices.Workspace
{
    public sealed class Document
    {
        private readonly SourceText _sourceText;
        private readonly Func<SourceText, CancellationToken, SyntaxTreeBase> _compileFunc;
        private readonly AsyncLazy<SyntaxTreeBase> _lazySyntaxTree;

        public Document(SourceText sourceText, Func<SourceText, CancellationToken, SyntaxTreeBase> compileFunc)
        {
            _sourceText = sourceText;
            _compileFunc = compileFunc;

            _lazySyntaxTree = new AsyncLazy<SyntaxTreeBase>(ct => Task.Run(() => compileFunc(sourceText, ct), ct), true);
        }

        public Task<SyntaxTreeBase> GetSyntaxTreeAsync(CancellationToken cancellationToken)
        {
            return _lazySyntaxTree.GetValueAsync(cancellationToken);
        }

        public Document WithSourceText(SourceText sourceText)
        {
            return new Document(sourceText, _compileFunc);
        }
    }
}
