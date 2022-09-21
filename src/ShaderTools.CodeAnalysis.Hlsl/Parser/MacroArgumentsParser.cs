using System.Collections.Generic;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
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

            var arguments = new List<SyntaxNodeBase>();

            CommaIsSeparatorStack.Push(true);

            try
            {
                var currentArg = new List<SyntaxToken>();
                var parenStack = 0;
                while ((Current.Kind != SyntaxKind.CloseParenToken || parenStack > 0) && Current.Kind != SyntaxKind.EndOfFileToken)
                {
                    switch (Current.Kind)
                    {
                        case SyntaxKind.OpenParenToken:
                            CommaIsSeparatorStack.Push(false);
                            parenStack++;
                            currentArg.Add(NextToken());
                            break;
                        case SyntaxKind.CloseParenToken:
                            CommaIsSeparatorStack.Pop();
                            parenStack--;
                            currentArg.Add(NextToken());
                            break;
                        case SyntaxKind.EllipsisToken:
                            arguments.Add(Match(SyntaxKind.EllipsisToken));
                            currentArg.Add(NextToken());
                            break;
                        case SyntaxKind.CommaToken:
                            if (CommaIsSeparatorStack.Peek() == false)
                                goto default;
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
            }
            finally
            {
                CommaIsSeparatorStack.Pop();
            }

            var argumentList = new SeparatedSyntaxList<MacroArgumentSyntax>(arguments);

            var closeParen = Match(SyntaxKind.CloseParenToken);

            return new MacroArgumentListSyntax(openParen, argumentList, closeParen);
        }
    }
}