using System.Collections.Generic;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    internal sealed class StatementBinder
    {
        public StatementBinder(ILocalSymbolTable symbolTable, MemberSymbol memberContext, List<Diagnostic> diagnostics)
        {
            
        }

        public BoundStatement BuildStatement(StatementSyntax statementNode)
        {
            throw new System.NotImplementedException();
        }
    }
}