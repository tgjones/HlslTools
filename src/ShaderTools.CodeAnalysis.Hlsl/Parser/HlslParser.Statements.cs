using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ShaderTools.CodeAnalysis.Hlsl.Diagnostics;
using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal partial class HlslParser
	{
        public StatementSyntax ParseStatement()
        {
            var current = _tokenIndex;

            // First, try to parse as a non-declaration statement. If the statement is a single
            // expression then we only allow legal expression statements. (That is, "new C();",
            // "C();", "x = y;" and so on.)
            var result = ParseStatementNoDeclaration();
            if (result != null && current != _tokenIndex)
                return result;

            // If that failed, parse as a declaration.
            return ParseDeclarationStatement();
        }

        private VariableDeclarationStatementSyntax ParseVariableDeclarationStatement()
        {
            var variableDeclaration = ParseVariableDeclaration();
            var semicolon = Match(SyntaxKind.SemiToken);
            return new VariableDeclarationStatementSyntax(variableDeclaration, semicolon);
        }

        private void ParseDeclarationModifiers(List<SyntaxToken> list)
        {
            while (SyntaxFacts.IsDeclarationModifier(Current))
                list.Add(NextToken());
        }

        private VariableDeclarationSyntax ParseVariableDeclaration()
        {
            TypeSyntax type;
            var mods = new List<SyntaxToken>();
            var variables = new List<SyntaxNodeBase>();
            ParseDeclarationModifiers(mods);
            ParseVariableDeclaration(out type, variables);
            return new VariableDeclarationSyntax(mods, type, new SeparatedSyntaxList<VariableDeclaratorSyntax>(variables));
        }

        private void ParseVariableDeclaration(out TypeSyntax type, List<SyntaxNodeBase> variables)
        {
            type = ParseType(false);
            ParseVariableDeclarators(type, variables, variableDeclarationsExpected: true);
        }

        private StatementSyntax ParseDeclarationStatement()
        {
            var attributes = ParseAttributes();

            var typedefKeyword = NextTokenIf(SyntaxKind.TypedefKeyword);

            var mods = new List<SyntaxToken>();
            var variables = new List<SyntaxNodeBase>();
            ParseDeclarationModifiers(mods);
            var type = ParseType(false);

            if (typedefKeyword != null)
            {
                ParseTypeAliases(variables);
                var semi = Match(SyntaxKind.SemiToken);
                return new TypedefStatementSyntax(typedefKeyword, mods, type, new SeparatedSyntaxList<TypeAliasSyntax>(variables), semi);
            }

            if (type is TypeDefinitionSyntax && (Current.Kind == SyntaxKind.SemiToken || Current.Kind == SyntaxKind.EndOfFileToken))
            {
                var semi = Match(SyntaxKind.SemiToken);
                return new TypeDeclarationStatementSyntax(mods, (TypeDefinitionSyntax) type, semi);
            }
            else
            {
                ParseVariableDeclarators(type, variables, variableDeclarationsExpected: true);
                var semi = Match(SyntaxKind.SemiToken);
                var variableDeclaration = new VariableDeclarationSyntax(mods, type, new SeparatedSyntaxList<VariableDeclaratorSyntax>(variables));
                return new VariableDeclarationStatementSyntax(attributes, variableDeclaration, semi);
            }
        }

        private void ParseTypeAliases(List<SyntaxNodeBase> variables)
        {
            variables.Add(ParseTypeAlias());

            while (true)
            {
                if (Current.Kind == SyntaxKind.SemiToken)
                    break;

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    variables.Add(Match(SyntaxKind.CommaToken));
                    variables.Add(ParseTypeAlias());
                    continue;
                }

                break;
            }
        }

        private TypeAliasSyntax ParseTypeAlias()
        {
            var name = Match(SyntaxKind.IdentifierToken);

            var arrayRankSpecifiers = new List<ArrayRankSpecifierSyntax>();
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                ParseArrayRankSpecifiers(arrayRankSpecifiers, false);

            var qualifiers = new List<VariableDeclaratorQualifierSyntax>();
            while (Current.Kind == SyntaxKind.ColonToken)
            {
                if (IsPossibleVariableDeclaratorQualifier(Lookahead))
                {
                    qualifiers.Add(ParseVariableDeclaratorQualifier());
                }
                else
                {
                    var action = SkipBadTokens(
                        p => !p.IsPossibleVariableDeclaratorQualifier(Current),
                        p => p.Current.Kind == SyntaxKind.EqualsToken || p.Current.Kind == SyntaxKind.OpenBraceToken || p.IsTerminator(),
                        SyntaxKind.RegisterKeyword);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            AnnotationsSyntax annotations = null;
            if (Current.Kind == SyntaxKind.LessThanToken)
                annotations = ParseAnnotations();

            return new TypeAliasSyntax(name, arrayRankSpecifiers, qualifiers, annotations);
        }

        private void ParseVariableDeclarators(TypeSyntax type, List<SyntaxNodeBase> variables, bool variableDeclarationsExpected)
        {
            variables.Add(ParseVariableDeclarator(type));

            while (true)
            {
                if (Current.Kind == SyntaxKind.SemiToken)
                    break;

                if (Current.Kind == SyntaxKind.CommaToken)
                {
                    variables.Add(Match(SyntaxKind.CommaToken));
                    variables.Add(ParseVariableDeclarator(type));
                    continue;
                }

                break;
            }
        }

        private VariableDeclaratorSyntax ParseVariableDeclarator(TypeSyntax parentType, bool isExpressionContext = false)
        {
            if (!isExpressionContext)
            {
                // Check for the common pattern of:
                //
                // C                    //<-- here
                // Console.WriteLine();
                //
                // Standard greedy parsing will assume that this should be parsed as a variable
                // declaration: "C Console".  We want to avoid that as it can confused parts of the
                // system further up.  So, if we see certain things following the identifier, then we can
                // assume it's not the actual name.  
                // 
                // So, if we're after a newline and we see a name followed by the list below, then we
                // assume that we're accidently consuming too far into the next statement.
                //
                // <dot>, <arrow>, any binary operator (except =), <question>.  None of these characters
                // are allowed in a normal variable declaration.  This also provides a more useful error
                // message to the user.  Instead of telling them that a semicolon is expected after the
                // following token, then instead get a useful message about an identifier being missing.
                // The above list prevents:
                //
                // C                    //<-- here
                // Console.WriteLine();
                //
                // C                    //<-- here 
                // Console->WriteLine();
                //
                // C 
                // A + B; // etc.
                //
                // C 
                // A ? B : D;
                var resetPoint = GetResetPoint();
                try
                {
                    var currentTokenKind = Current.Kind;
                    if (currentTokenKind == SyntaxKind.IdentifierToken && !parentType.IsMissing)
                    {
                        var isAfterNewLine = parentType.GetLastChildToken().TrailingTrivia.Any(t => t.Kind == SyntaxKind.EndOfLineTrivia);
                        if (isAfterNewLine)
                        {
                            NextToken();
                            currentTokenKind = Current.Kind;

                            var isNonEqualsBinaryToken =
                                currentTokenKind != SyntaxKind.EqualsToken &&
                                SyntaxFacts.IsBinaryExpression(currentTokenKind);

                            if (currentTokenKind == SyntaxKind.DotToken ||
                                isNonEqualsBinaryToken)
                            {
                                var missingIdentifier = InsertMissingToken(SyntaxKind.IdentifierToken);
                                return new VariableDeclaratorSyntax(missingIdentifier, 
                                    new List<ArrayRankSpecifierSyntax>(), 
                                    new List<VariableDeclaratorQualifierSyntax>(), 
                                    null, null);
                            }
                        }
                    }
                }
                finally
                {
                    Reset(ref resetPoint);
                }
            }

            var name = Match(SyntaxKind.IdentifierToken);

            var arrayRankSpecifiers = new List<ArrayRankSpecifierSyntax>();
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                ParseArrayRankSpecifiers(arrayRankSpecifiers, false);

            var qualifiers = new List<VariableDeclaratorQualifierSyntax>();
            while (Current.Kind == SyntaxKind.ColonToken)
            {
                if (IsPossibleVariableDeclaratorQualifier(Lookahead))
                {
                    qualifiers.Add(ParseVariableDeclaratorQualifier());
                }
                else
                {
                    var action = SkipBadTokens(
                        p => !p.IsPossibleVariableDeclaratorQualifier(Current),
                        p => p.Current.Kind == SyntaxKind.EqualsToken || p.Current.Kind == SyntaxKind.OpenBraceToken || p.IsTerminator(),
                        SyntaxKind.RegisterKeyword);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            AnnotationsSyntax annotations = null;
            if (Current.Kind == SyntaxKind.LessThanToken)
                annotations = ParseAnnotations();

            InitializerSyntax initializer = null;
            if (Current.Kind == SyntaxKind.EqualsToken)
            {
                if (Lookahead.Kind == SyntaxKind.SamplerStateLegacyKeyword)
                {
                    initializer = ParseSamplerStateInitializer();
                }
                else
                {
                    var equals = NextToken();
                    var init = ParseVariableInitializer();
                    initializer = new EqualsValueClauseSyntax(equals, init);
                }
            }
            else if (Current.Kind == SyntaxKind.OpenBraceToken)
            {
                if (Lookahead.Kind == SyntaxKind.OpenBraceToken)
                    initializer = ParseStateArrayInitializer();
                else
                    initializer = ParseStateInitializer();
            }

            return new VariableDeclaratorSyntax(name, arrayRankSpecifiers, qualifiers, annotations, initializer);
        }

        private SamplerStateInitializerSyntax ParseSamplerStateInitializer()
        {
            var equals = Match(SyntaxKind.EqualsToken);
            var samplerState = Match(SyntaxKind.SamplerStateLegacyKeyword);
            var stateInitializer = ParseStateInitializer();

            return new SamplerStateInitializerSyntax(equals, samplerState, stateInitializer);
        }

        private StateArrayInitializerSyntax ParseStateArrayInitializer()
        {
            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var initializers = new List<SyntaxNodeBase>();
            while (Current.Kind != SyntaxKind.CloseBraceToken)
            {
                if (Current.Kind == SyntaxKind.OpenBraceToken)
                {
                    initializers.Add(ParseStateInitializer());
                    if (Current.Kind != SyntaxKind.CloseBraceToken)
                        initializers.Add(Match(SyntaxKind.CommaToken));
                }
                else
                {
                    var action = SkipBadTokens(
                        p => Current.Kind != SyntaxKind.OpenBraceToken,
                        p => p.IsTerminator(),
                        SyntaxKind.CloseBraceToken);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            var closeBrace = Match(SyntaxKind.CloseBraceToken);

            return new StateArrayInitializerSyntax(openBrace, new SeparatedSyntaxList<StateInitializerSyntax>(initializers), closeBrace);
        }

        private StateInitializerSyntax ParseStateInitializer()
        {
            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var properties = new List<StatePropertySyntax>();
            while (Current.Kind != SyntaxKind.CloseBraceToken)
            {
                if (IsPossibleStateProperty())
                {
                    properties.Add(ParseStateProperty());
                }
                else
                {
                    var action = SkipBadTokens(
                        p => !p.IsPossibleStateProperty(),
                        p => p.IsTerminator(),
                        SyntaxKind.CloseBraceToken);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            var closeBrace = Match(SyntaxKind.CloseBraceToken);

            return new StateInitializerSyntax(openBrace, properties, closeBrace);
        }

        private bool IsPossibleStateProperty()
        {
            return Current.Kind == SyntaxKind.IdentifierToken; // && Lookahead.Kind == SyntaxKind.EqualsToken;
        }

        private StatePropertySyntax ParseStateProperty()
        {
            var name = Match(SyntaxKind.IdentifierToken);

            ArrayRankSpecifierSyntax arrayRankSpecifier = null;
            if (Current.Kind == SyntaxKind.OpenBracketToken)
                arrayRankSpecifier = ParseArrayRankSpecifier(true);

            var equals = Match(SyntaxKind.EqualsToken);
            var lessThan = NextTokenIf(SyntaxKind.LessThanToken);

            _allowGreaterThanTokenAroundRhsExpression = true;
            _allowLinearAndPointAsIdentifiers = true;
            ExpressionSyntax value;
            try
            {
                value = ParseExpression();
            }
            finally
            {
                _allowLinearAndPointAsIdentifiers = false;
                _allowGreaterThanTokenAroundRhsExpression = false;
            }
            
            var greaterThan = NextTokenIf(SyntaxKind.GreaterThanToken);
            var semicolon = Match(SyntaxKind.SemiToken);

            return new StatePropertySyntax(name, arrayRankSpecifier, equals, lessThan, value, greaterThan, semicolon);
        }

        private bool IsPossibleVariableDeclaratorQualifier(SyntaxToken token)
        {
            switch (token.Kind)
            {
                case SyntaxKind.RegisterKeyword:
                case SyntaxKind.PackoffsetKeyword:
                case SyntaxKind.IdentifierToken:
                case SyntaxKind.SemiToken:
                    return true;

                default:
                    return false;
            }
        }

        private VariableDeclaratorQualifierSyntax ParseVariableDeclaratorQualifier()
        {
            switch (Lookahead.Kind)
            {
                case SyntaxKind.RegisterKeyword:
                    return ParseRegisterLocation();
                case SyntaxKind.PackoffsetKeyword:
                    return ParsePackOffsetLocation();
                default:
                    return ParseSemantic();
            }
        }

        private ExpressionSyntax ParseVariableInitializer()
        {
            CommaIsSeparatorStack.Push(true);

            try
            {
                switch (Current.Kind)
                {
                    case SyntaxKind.OpenBraceToken:
                        return ParseArrayInitializer();
                    default:
                        return ParseExpression();
                }
            }
            finally
            {
                CommaIsSeparatorStack.Pop();
            }
        }

        private ArrayInitializerExpressionSyntax ParseArrayInitializer()
        {
            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var list = new List<SyntaxNodeBase>();
            while (Current.Kind != SyntaxKind.CloseBraceToken)
            {
                if (IsPossibleVariableInitializer())
                {
                    list.Add(ParseVariableInitializer());
                    if (Current.Kind != SyntaxKind.CloseBraceToken)
                    {
                        if (Current.Kind == SyntaxKind.CommaToken)
                        {
                            list.Add(Match(SyntaxKind.CommaToken));
                        }
                        else
                        {
                            var action = SkipBadTokens(
                                p => p.Current.Kind != SyntaxKind.CommaToken,
                                p => p.IsTerminator(),
                                SyntaxKind.CommaToken);
                            if (action == PostSkipAction.Abort)
                                break;
                            list.Add(Match(SyntaxKind.CommaToken));
                        }
                    }
                }
                else
                {
                    var action = SkipBadTokens(
                        p => !p.IsPossibleVariableInitializer(),
                        p => p.IsTerminator(),
                        SyntaxKind.CloseBraceToken);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            var closeBrace = Match(SyntaxKind.CloseBraceToken);

            return new ArrayInitializerExpressionSyntax(openBrace, new SeparatedSyntaxList<ExpressionSyntax>(list), closeBrace);
        }

        private bool IsPossibleVariableInitializer()
        {
            return Current.Kind == SyntaxKind.OpenBraceToken || IsPossibleExpression();
        }

        /// <summary>
        /// Parses any statement but a declaration statement. Returns null if the lookahead looks like a declaration.
        /// </summary>
        /// <remarks>
        /// Variable declarations in global code are parsed as field declarations so we need to fallback if we encounter a declaration statement.
        /// </remarks>
        private StatementSyntax ParseStatementNoDeclaration()
        {
            if (IsPossibleDeclarationStatement())
                return null;

            var attributes = ParseAttributes();

            switch (Current.Kind)
            {
                case SyntaxKind.BreakKeyword:
                    return ParseBreakStatement(attributes);
                case SyntaxKind.ContinueKeyword:
                    return ParseContinueStatement(attributes);
                case SyntaxKind.ConstKeyword:
                    return null;
                case SyntaxKind.DiscardKeyword:
                    return ParseDiscardStatement(attributes);
                case SyntaxKind.DoKeyword:
                    return ParseDoStatement(attributes);
                case SyntaxKind.ForKeyword:
                    return ParseForStatement(attributes);
                case SyntaxKind.IfKeyword:
                    return ParseIfStatement(attributes);
                case SyntaxKind.ReturnKeyword:
                    return ParseReturnStatement(attributes);
                case SyntaxKind.SwitchKeyword:
                    return ParseSwitchStatement(attributes);
                case SyntaxKind.WhileKeyword:
                    return ParseWhileStatement(attributes);
                case SyntaxKind.OpenBraceToken:
                    return ParseBlock(attributes);
                case SyntaxKind.SemiToken:
                    return new EmptyStatementSyntax(attributes, NextToken());
                default:
                    return ParseExpressionStatement(attributes);
            }
        }

        private bool IsPossibleCBufferOrTBuffer()
        {
            var resetPoint = GetResetPoint();
            try
            {
                ParseAttributes();

                return (Current.Kind == SyntaxKind.CBufferKeyword || Current.Kind == SyntaxKind.TBufferKeyword);
            }
            finally
            {
                Reset(ref resetPoint);
            }
        }

        private bool IsPossibleFunctionDeclaration()
        {
            var resetPoint = GetResetPoint();
            try
            {
                ParseAttributes();

                var modifiers = new List<SyntaxToken>();
                ParseDeclarationModifiers(modifiers);

                var st = ScanType();

                if (st == ScanTypeFlags.NotType)
                    return false;

                if (Current.Kind != SyntaxKind.IdentifierToken)
                    return false;

                NextToken();

                while (Current.Kind == SyntaxKind.ColonColonToken)
                {
                    NextToken();
                    if (Current.Kind != SyntaxKind.IdentifierToken)
                        return false;
                    NextToken();
                }

                if (Current.Kind == SyntaxKind.OpenParenToken) // Indicates a function, not variable declaration.
                    return true;

                return false;
            }
            finally
            {
                Reset(ref resetPoint);
            }
        }

        private bool IsPossibleVariableDeclarationStatement()
        {
            return IsPossibleDeclarationStatement();
        }

        private bool IsPossibleDeclarationStatement()
        {
            var tk = Current.Kind;

            // Although "<identifier> <literal>" is invalid, it's common enough that we try to parse it anyway, and report on the error.
            if (tk == SyntaxKind.IdentifierToken && (Lookahead.Kind == SyntaxKind.IdentifierToken || Lookahead.Kind.IsLiteral()))
                return true;

            if (IsPossibleAttributeSpecifierList())
                return true;

            switch (Current.Kind)
            {
                case SyntaxKind.ClassKeyword:
                case SyntaxKind.StructKeyword:
                case SyntaxKind.InterfaceKeyword:
                case SyntaxKind.TypedefKeyword:
                    return true;
            }

            var resetPoint = GetResetPoint();
            try
            {
                var modifiers = new List<SyntaxToken>();
                ParseDeclarationModifiers(modifiers);

                var st = ScanType();

                if (st == ScanTypeFlags.NotType)
                    return false;

                // Although "<type> <literal>" is invalid, it's common enough that we try to parse it anyway, and report on the error.
                if (Current.Kind != SyntaxKind.IdentifierToken && Current.Kind != SyntaxKind.SemiToken && !Current.Kind.IsLiteral())
                    return false;

                if (Lookahead.Kind == SyntaxKind.OpenParenToken)
                    return false;

                return true;
            }
            finally
            {
                Reset(ref resetPoint);
            }
        }

        private BlockSyntax ParseBlock(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var statements = new List<StatementSyntax>();
            ParseStatements(statements, stopOnSwitchSections: false);

            var closeBrace = Match(SyntaxKind.CloseBraceToken);

            return new BlockSyntax(attributes, openBrace, statements, closeBrace);
        }

        private BreakStatementSyntax ParseBreakStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var breakKeyword = Match(SyntaxKind.BreakKeyword);
            var semicolon = Match(SyntaxKind.SemiToken);
            return new BreakStatementSyntax(attributes, breakKeyword, semicolon);
        }

        private ContinueStatementSyntax ParseContinueStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var continueKeyword = Match(SyntaxKind.ContinueKeyword);
            var semicolon = Match(SyntaxKind.SemiToken);
            return new ContinueStatementSyntax(attributes, continueKeyword, semicolon);
        }

        private DiscardStatementSyntax ParseDiscardStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var discardKeyword = Match(SyntaxKind.DiscardKeyword);
            var semicolon = Match(SyntaxKind.SemiToken);
            return new DiscardStatementSyntax(attributes, discardKeyword, semicolon);
        }

        private DoStatementSyntax ParseDoStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @do = Match(SyntaxKind.DoKeyword);
            var statement = ParseEmbeddedStatement();
            var @while = Match(SyntaxKind.WhileKeyword);
            var openParen = Match(SyntaxKind.OpenParenToken);
            var expression = ParseExpression();
            var closeParen = Match(SyntaxKind.CloseParenToken);
            var semicolon = Match(SyntaxKind.SemiToken);
            return new DoStatementSyntax(attributes, @do, statement, @while, openParen, expression, closeParen, semicolon);
        }

        private StatementSyntax ParseEmbeddedStatement()
        {
            var statement = ParseStatement();

            // An "embedded" statement is simply a statement that is not a declaration statement.
            // Parse a normal statement and post-check for the error case.
            if (statement != null && statement.Kind == SyntaxKind.VariableDeclarationStatement)
                statement = WithDiagnostic(statement, DiagnosticId.BadEmbeddedStatement);

            return statement;
        }

        private ExpressionStatementSyntax ParseExpressionStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var expression = ParseExpression();
            var semicolon = Match(SyntaxKind.SemiToken);
            return new ExpressionStatementSyntax(attributes, expression, semicolon);
        }

        private ForStatementSyntax ParseForStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @for = Match(SyntaxKind.ForKeyword);
            var openParen = Match(SyntaxKind.OpenParenToken);

            var resetPoint = GetResetPoint();
            ExpressionSyntax initializer = null;
            ExpressionSyntax incrementor = null;

            // Here can be either a declaration or an expression statement list.  Scan
            // for a declaration first.
            ScanTypeFlags st = ScanType();

            VariableDeclarationSyntax decl = null;

            if (st != ScanTypeFlags.NotType && Current.Kind == SyntaxKind.IdentifierToken)
            {
                Reset(ref resetPoint);
                decl = ParseVariableDeclaration();
            }
            else
            {
                // Not a type followed by an identifier, so it must be an expression list.
                Reset(ref resetPoint);
                if (Current.Kind != SyntaxKind.SemiToken)
                    initializer = ParseExpression();
            }

            var semi = Match(SyntaxKind.SemiToken);

            ExpressionSyntax condition = null;
            if (Current.Kind != SyntaxKind.SemiToken)
                condition = ParseExpression();

            var semi2 = Match(SyntaxKind.SemiToken);

            if (Current.Kind != SyntaxKind.CloseParenToken)
                incrementor = ParseExpression();

            var closeParen = Match(SyntaxKind.CloseParenToken);
            var statement = ParseEmbeddedStatement();

            return new ForStatementSyntax(attributes, @for, openParen, decl,
                initializer, semi, condition, semi2, 
                incrementor, closeParen, statement);
        }

        private void ParseStatements(List<StatementSyntax> statements, bool stopOnSwitchSections)
        {
            var saveTerm = _termState;
            _termState |= TerminatorState.IsPossibleStatementStartOrStop; // partial statements can abort if a new statement starts
            if (stopOnSwitchSections)
                _termState |= TerminatorState.IsSwitchSectionStart;

            while (Current.Kind != SyntaxKind.CloseBraceToken
                && Current.Kind != SyntaxKind.EndOfFileToken
                && !(stopOnSwitchSections && IsPossibleSwitchSection()))
            {
                if (IsPossibleStatement())
                {
                    statements.Add(ParseStatement());
                }
                else
                {
                    var action = SkipBadStatementListTokens(SyntaxKind.CloseBraceToken);
                    if (action == PostSkipAction.Abort)
                        break;
                }
            }

            _termState = saveTerm;
        }

        private IfStatementSyntax ParseIfStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @if = Match(SyntaxKind.IfKeyword);
            var openParen = Match(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = Match(SyntaxKind.CloseParenToken);
            var statement = ParseEmbeddedStatement();
            ElseClauseSyntax @else = null;
            if (Current.Kind == SyntaxKind.ElseKeyword)
            {
                var elseToken = Match(SyntaxKind.ElseKeyword);
                var elseStatement = ParseEmbeddedStatement();
                @else = new ElseClauseSyntax(elseToken, elseStatement);
            }

            return new IfStatementSyntax(attributes, @if, openParen, condition, closeParen, statement, @else);
        }

        private ReturnStatementSyntax ParseReturnStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @return = Match(SyntaxKind.ReturnKeyword);
            ExpressionSyntax arg = null;
            if (Current.Kind != SyntaxKind.SemiToken)
                arg = ParseExpression();

            var semicolon = Match(SyntaxKind.SemiToken);
            return new ReturnStatementSyntax(attributes, @return, arg, semicolon);
        }

        private bool IsPossibleSwitchSection()
        {
            return Current.Kind == SyntaxKind.CaseKeyword || Current.Kind == SyntaxKind.DefaultKeyword;
        }

        private SwitchStatementSyntax ParseSwitchStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @switch = Match(SyntaxKind.SwitchKeyword);
            var openParen = Match(SyntaxKind.OpenParenToken);
            var expression = ParseExpression();
            var closeParen = Match(SyntaxKind.CloseParenToken);
            var openBrace = Match(SyntaxKind.OpenBraceToken);

            var sections = new List<SwitchSectionSyntax>();
            while (IsPossibleSwitchSection())
            {
                var swcase = ParseSwitchSection();
                sections.Add(swcase);
            }

            var closeBrace = Match(SyntaxKind.CloseBraceToken);
            return new SwitchStatementSyntax(attributes, @switch, openParen, expression, closeParen, openBrace, sections, closeBrace);
        }

        private SwitchSectionSyntax ParseSwitchSection()
        {
            // First, parse case label(s)
            var labels = new List<SwitchLabelSyntax>();
            var statements = new List<StatementSyntax>();
            do
            {
                SyntaxToken specifier;
                SwitchLabelSyntax label;
                SyntaxToken colon;
                if (Current.Kind == SyntaxKind.CaseKeyword)
                {
                    ExpressionSyntax expression;
                    specifier = NextToken();
                    if (Current.Kind == SyntaxKind.ColonToken)
                    {
                        expression = CreateMissingIdentifierName();
                        expression = WithDiagnostic(expression, DiagnosticId.ConstantExpected);
                    }
                    else
                    {
                        expression = ParseExpression();
                    }
                    colon = Match(SyntaxKind.ColonToken);
                    label = new CaseSwitchLabelSyntax(specifier, expression, colon);
                }
                else
                {
                    Debug.Assert(Current.Kind == SyntaxKind.DefaultKeyword);
                    specifier = Match(SyntaxKind.DefaultKeyword);
                    colon = Match(SyntaxKind.ColonToken);
                    label = new DefaultSwitchLabelSyntax(specifier, colon);
                }

                labels.Add(label);
            }
            while (IsPossibleSwitchSection());

            // Next, parse statement list stopping for new sections
            ParseStatements(statements, true);

            return new SwitchSectionSyntax(labels, statements);
        }

        private WhileStatementSyntax ParseWhileStatement(List<AttributeDeclarationSyntaxBase> attributes)
        {
            var @while = Match(SyntaxKind.WhileKeyword);
            var openParen = Match(SyntaxKind.OpenParenToken);
            var condition = ParseExpression();
            var closeParen = Match(SyntaxKind.CloseParenToken);
            var statement = ParseEmbeddedStatement();
            return new WhileStatementSyntax(attributes, @while, openParen, condition, closeParen, statement);
        }
    }
}