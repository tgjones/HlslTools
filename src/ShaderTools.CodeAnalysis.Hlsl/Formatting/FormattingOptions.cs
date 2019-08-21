using System;

namespace ShaderTools.CodeAnalysis.Hlsl.Formatting
{
    public class IndentationOptions
    {
        public bool IndentBlockContents { get; set; } = true;
        public bool IndentOpenAndCloseBraces { get; set; }
        public bool IndentCaseContents { get; set; } = true;
        public bool IndentCaseLabels { get; set; } = true;

        public PreprocessorDirectivePosition PreprocessorDirectivePosition { get; set; } = PreprocessorDirectivePosition.MoveToLeftmostColumn;
    }

    public class SpacingOptions
    {
        public bool FunctionDeclarationInsertSpaceAfterFunctionName { get; set; }
        public bool FunctionDeclarationInsertSpaceWithinArgumentListParentheses { get; set; }
        public bool FunctionDeclarationInsertSpaceWithinEmptyArgumentListParentheses { get; set; }

        public bool FunctionCallInsertSpaceAfterFunctionName { get; set; }
        public bool FunctionCallInsertSpaceWithinArgumentListParentheses { get; set; }
        public bool FunctionCallInsertSpaceWithinEmptyArgumentListParentheses { get; set; }

        public bool InsertSpaceAfterKeywordsInControlFlowStatements { get; set; } = true;
        public bool InsertSpacesWithinParenthesesOfExpressions { get; set; }
        public bool InsertSpacesWithinParenthesesOfTypeCasts { get; set; }
        public bool InsertSpacesWithinParenthesesOfControlFlowStatements { get; set; }
        public bool InsertSpacesWithinParenthesesOfRegisterOrPackOffsetQualifiers { get; set; }
        public bool InsertSpacesWithinArrayInitializerBraces { get; set; } = true;
        public bool InsertSpaceAfterCast { get; set; } = true;

        public bool InsertSpaceBeforeOpenSquareBracket { get; set; }
        public bool InsertSpaceWithinEmptySquareBrackets { get; set; }
        public bool InsertSpacesWithinSquareBrackets { get; set; }

        public bool InsertSpaceBeforeColonForBaseOrInterfaceInTypeDeclaration { get; set; } = true;
        public bool InsertSpaceAfterColonForBaseOrInterfaceInTypeDeclaration { get; set; } = true;
        public bool InsertSpaceBeforeColonForSemanticOrRegisterOrPackOffset { get; set; } = true;
        public bool InsertSpaceAfterColonForSemanticOrRegisterOrPackOffset { get; set; } = true;
        public bool InsertSpaceBeforeComma { get; set; }
        public bool InsertSpaceAfterComma { get; set; } = true;
        public bool InsertSpaceBeforeDot { get; set; }
        public bool InsertSpaceAfterDot { get; set; }
        public bool InsertSpaceBeforeSemicolonInForStatement { get; set; }
        public bool InsertSpaceAfterSemicolonInForStatement { get; set; } = true;

        public BinaryOperatorSpaces BinaryOperatorSpaces { get; set; } = BinaryOperatorSpaces.InsertSpaces;
    }

    public class NewLinesOptions
    {
        public OpenBracesPosition OpenBracePositionForTypes { get; set; } = OpenBracesPosition.MoveToNewLine;
        public OpenBracesPosition OpenBracePositionForTechniquesAndPasses { get; set; } = OpenBracesPosition.MoveToNewLine;
        public OpenBracesPosition OpenBracePositionForFunctions { get; set; } = OpenBracesPosition.MoveToNewLine;
        public OpenBracesPosition OpenBracePositionForControlBlocks { get; set; } = OpenBracesPosition.MoveToNewLine;
        public OpenBracesPosition OpenBracePositionForStateBlocks { get; set; } = OpenBracesPosition.MoveToNewLine;
        public OpenBracesPosition OpenBracePositionForArrayInitializers { get; set; } = OpenBracesPosition.MoveToNewLine;

        public bool PlaceElseOnNewLine { get; set; } = true;
    }

    public class FormattingOptions
    {
        public IndentationOptions Indentation { get; set; }
        public NewLinesOptions NewLines { get; set; }
        public SpacingOptions Spacing { get; set; }

        public bool UseTabs { get; set; }
        public int SpacesPerIndent { get; set; }
        public string NewLine { get; set; }

        public FormattingOptions()
        {
            Indentation = new IndentationOptions();
            NewLines = new NewLinesOptions();
            Spacing = new SpacingOptions();

            UseTabs = false;
            SpacesPerIndent = 4;
            NewLine = Environment.NewLine;
        }
    }

    public enum PreprocessorDirectivePosition
    {
        OneIndentToLeft,
        MoveToLeftmostColumn,
        LeaveIndented
    }

    public enum OpenBracesPosition
    {
        MoveToNewLine,
        KeepOnSameLineAndPrependSpace,
        DoNotMove
    }

    public enum BinaryOperatorSpaces
    {
        RemoveSpaces,
        InsertSpaces
    }
}