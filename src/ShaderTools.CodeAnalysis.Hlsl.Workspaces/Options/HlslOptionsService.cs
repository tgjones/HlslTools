using System.Composition;
using ShaderTools.CodeAnalysis.Hlsl.Formatting;
using ShaderTools.CodeAnalysis.Options;

namespace ShaderTools.CodeAnalysis.Hlsl.Options
{
    [Export(typeof(IHlslOptionsService))]
    internal sealed class HlslOptionsService : IHlslOptionsService
    {
        public FormattingOptions GetFormattingOptions(OptionSet options)
        {
            return new FormattingOptions
            {
                Indentation = new IndentationOptions
                {
                    IndentBlockContents = options.GetOption(HlslFormattingOptions.IndentBlockContents),
                    IndentCaseContents = options.GetOption(HlslFormattingOptions.IndentCaseContents),
                    IndentCaseLabels = options.GetOption(HlslFormattingOptions.IndentCaseLabels),
                    IndentOpenAndCloseBraces = options.GetOption(HlslFormattingOptions.IndentOpenAndCloseBraces),
                    PreprocessorDirectivePosition = options.GetOption(HlslFormattingOptions.PreprocessorDirectivePosition)
                },

                NewLines = new NewLinesOptions
                {
                    OpenBracePositionForArrayInitializers = options.GetOption(HlslFormattingOptions.OpenBracePositionForArrayInitializers),
                    OpenBracePositionForControlBlocks = options.GetOption(HlslFormattingOptions.OpenBracePositionForControlBlocks),
                    OpenBracePositionForFunctions = options.GetOption(HlslFormattingOptions.OpenBracePositionForFunctions),
                    OpenBracePositionForStateBlocks = options.GetOption(HlslFormattingOptions.OpenBracePositionForStateBlocks),
                    OpenBracePositionForTechniquesAndPasses = options.GetOption(HlslFormattingOptions.OpenBracePositionForTechniques),
                    OpenBracePositionForTypes = options.GetOption(HlslFormattingOptions.OpenBracePositionForTypes),
                    PlaceElseOnNewLine = options.GetOption(HlslFormattingOptions.PlaceElseOnNewLine)
                },

                Spacing = new SpacingOptions
                {
                    BinaryOperatorSpaces = options.GetOption(HlslFormattingOptions.BinaryOperatorSpaces),
                    FunctionCallInsertSpaceAfterFunctionName = options.GetOption(HlslFormattingOptions.SpaceAfterFunctionCallName),
                    FunctionCallInsertSpaceWithinArgumentListParentheses = options.GetOption(HlslFormattingOptions.SpaceWithinFunctionCallParentheses),
                    FunctionCallInsertSpaceWithinEmptyArgumentListParentheses = options.GetOption(HlslFormattingOptions.SpaceWithinFunctionCallEmptyParentheses),
                    FunctionDeclarationInsertSpaceAfterFunctionName = options.GetOption(HlslFormattingOptions.SpaceAfterFunctionDeclarationName),
                    FunctionDeclarationInsertSpaceWithinArgumentListParentheses = options.GetOption(HlslFormattingOptions.SpaceWithinFunctionDeclarationParentheses),
                    FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses = options.GetOption(HlslFormattingOptions.SpaceWithinFunctionDeclarationEmptyParentheses),
                    InsertSpaceAfterCast = options.GetOption(HlslFormattingOptions.SpaceAfterTypeCast),
                    InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration = options.GetOption(HlslFormattingOptions.SpaceAfterColonInBaseTypeDeclaration),
                    InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset = options.GetOption(HlslFormattingOptions.SpaceAfterColonInSemanticOrRegisterOrPackOffset),
                    InsertSpaceAfterComma = options.GetOption(HlslFormattingOptions.SpaceAfterComma),
                    InsertSpaceAfterDot = options.GetOption(HlslFormattingOptions.SpaceAfterDot),
                    InsertSpaceAfterKeywordsInControlFlowStatements = options.GetOption(HlslFormattingOptions.SpaceAfterControlFlowStatementKeyword),
                    InsertSpaceAfterSemicolonInForStatement = options.GetOption(HlslFormattingOptions.SpaceAfterSemicolonsInForStatement),
                    InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration = options.GetOption(HlslFormattingOptions.SpaceBeforeColonInBaseTypeDeclaration),
                    InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset = options.GetOption(HlslFormattingOptions.SpaceBeforeColonInSemanticOrRegisterOrPackOffset),
                    InsertSpaceBeforeComma = options.GetOption(HlslFormattingOptions.SpaceBeforeComma),
                    InsertSpaceBeforeDot = options.GetOption(HlslFormattingOptions.SpaceBeforeDot),
                    InsertSpaceBeforeOpenSquareBracket = options.GetOption(HlslFormattingOptions.SpaceBeforeOpenSquareBracket),
                    InsertSpaceBeforeSemicolonInForStatement = options.GetOption(HlslFormattingOptions.SpaceBeforeSemicolonsInForStatement),
                    InsertSpacesWithinArrayInitializerBraces = options.GetOption(HlslFormattingOptions.SpaceWithinArrayInitializerBraces),
                    InsertSpacesWithinParenthesesOfControlFlowStatements = options.GetOption(HlslFormattingOptions.SpaceWithinControlFlowStatementParentheses),
                    InsertSpacesWithinParenthesesOfExpressions = options.GetOption(HlslFormattingOptions.SpaceWithinExpressionParentheses),
                    InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers = options.GetOption(HlslFormattingOptions.SpaceWithinRegisterOrPackOffsetParentheses),
                    InsertSpacesWithinParenthesesOfTypeCasts = options.GetOption(HlslFormattingOptions.SpaceWithinTypeCastParentheses),
                    InsertSpacesWithinSquareBrackets = options.GetOption(HlslFormattingOptions.SpaceWithinSquareBrackets),
                    InsertSpaceWithinEmptySquareBrackets = options.GetOption(HlslFormattingOptions.SpaceWithinEmptySquareBrackets)
                },

                UseTabs = options.GetOption(CodeAnalysis.Formatting.FormattingOptions.UseTabs, LanguageNames.Hlsl),
                SpacesPerIndent = options.GetOption(CodeAnalysis.Formatting.FormattingOptions.IndentationSize, LanguageNames.Hlsl)
            };
        }

        public FormattingOptions GetPrimaryWorkspaceFormattingOptions()
        {
            return GetFormattingOptions(PrimaryWorkspace.Workspace.Options);
        }
    }
}