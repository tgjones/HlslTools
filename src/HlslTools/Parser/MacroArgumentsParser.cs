using System.Collections.Generic;
using System.Linq;
using HlslTools.Syntax;

namespace HlslTools.Parser
{
    internal sealed class MacroArgumentsParser : HlslParser
    {
        public MacroArgumentsParser(ILexer lexer)
            : base(lexer, LexerMode.Syntax)
        {
            
        }

        public MacroArgumentListSyntax ParseArgumentList()
        {
            var openParen = Match(SyntaxKind.OpenParenToken);

            var arguments = new List<SyntaxNode>();

            var currentArg = new List<SyntaxToken>();
            var parenStack = 0;
            while ((Current.Kind != SyntaxKind.CloseParenToken || parenStack > 0) && Current.Kind != SyntaxKind.EndOfFileToken)
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.OpenParenToken:
                        parenStack++;
                        currentArg.Add(NextToken());
                        break;
                    case SyntaxKind.CloseParenToken:
                        parenStack--;
                        currentArg.Add(NextToken());
                        break;
                    case SyntaxKind.CommaToken:
                        arguments.Add(new MacroArgumentSyntax(currentArg));
                        currentArg = new List<SyntaxToken>();
                        arguments.Add(Match(SyntaxKind.CommaToken));
                        break;
                    default:
                        currentArg.Add(NextToken());
                        break;
                }
            }

            if (currentArg.Any())
                arguments.Add(new MacroArgumentSyntax(currentArg));

            var argumentList = new SeparatedSyntaxList<MacroArgumentSyntax>(arguments);

            var closeParen = Match(SyntaxKind.CloseParenToken);

            return new MacroArgumentListSyntax(openParen, argumentList, closeParen);
        }
    }
}