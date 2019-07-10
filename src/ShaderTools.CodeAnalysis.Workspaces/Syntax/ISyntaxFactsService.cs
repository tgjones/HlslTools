using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Syntax
{
    internal interface ISyntaxFactsService : ILanguageService
    {
        SourceFileSpan? GetFileSpanRoot(SyntaxNodeBase node);

        string GetKindText(ushort kind);

        bool IsBindableToken(ISyntaxToken token);

        /// <summary>
        /// Returns the parent node that binds to the symbols that the IDE prefers for features like
        /// Quick Info and Find All References. For example, if the token is part of the type of
        /// an object creation, the parenting object creation expression is returned so that binding
        /// will return constructor symbols.
        /// </summary>
        SyntaxNodeBase GetBindableParent(ISyntaxToken token);

        bool IsCaseSensitive { get; }

        bool IsIdentifierStartCharacter(char c);
        bool IsIdentifierPartCharacter(char c);
    }
}
