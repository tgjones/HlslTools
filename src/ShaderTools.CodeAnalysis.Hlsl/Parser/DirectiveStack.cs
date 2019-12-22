using ShaderTools.CodeAnalysis.Hlsl.Syntax;
using ShaderTools.CodeAnalysis.Hlsl.Util;

namespace ShaderTools.CodeAnalysis.Hlsl.Parser
{
    internal struct Directive
    {
        internal Directive(DirectiveTriviaSyntax node)
        {
            Node = node;
        }

        public SyntaxKind Kind => Node.Kind;

        public DirectiveTriviaSyntax Node { get; }

        public string GetIdentifier()
        {
            switch (Node.Kind)
            {
                case SyntaxKind.ObjectLikeDefineDirectiveTrivia:
                    return ((ObjectLikeDefineDirectiveTriviaSyntax)Node).Name.Text;
                case SyntaxKind.FunctionLikeDefineDirectiveTrivia:
                    return ((FunctionLikeDefineDirectiveTriviaSyntax)Node).Name.Text;
                case SyntaxKind.UndefDirectiveTrivia:
                    return ((UndefDirectiveTriviaSyntax)Node).Name.Text;
                default:
                    return null;
            }
        }

        internal bool IsActive => Node.IsActive;

        internal bool BranchTaken
        {
            get
            {
                var branching = Node as BranchingDirectiveTriviaSyntax;
                return branching != null && branching.BranchTaken;
            }
        }

        internal DirectiveTriviaSyntax BranchEnd
        {
            set
            {
                var branching = (BranchingDirectiveTriviaSyntax)Node;
                branching.BranchEnd = value;
            }
        }
    }

    internal enum DefineState
    {
        Defined,
        Undefined,
        Unspecified
    }

    internal struct DirectiveStack
    {
        public static readonly DirectiveStack Empty = new DirectiveStack(ConsList<Directive>.Empty);

        private readonly ConsList<Directive> _directives;

        private DirectiveStack(ConsList<Directive> directives)
        {
            _directives = directives;
        }

        public bool IsEmpty => _directives == ConsList<Directive>.Empty;

        public DefineState IsDefined(string id)
        {
            DefineDirectiveTriviaSyntax directive;
            return IsDefined(id, out directive);
        }

        public DefineState IsDefined(string id, out DefineDirectiveTriviaSyntax directive)
        {
            for (var current = _directives; current != null && current.Any(); current = current.Tail)
            {
                switch (current.Head.Kind)
                {
                    case SyntaxKind.ObjectLikeDefineDirectiveTrivia:
                    case SyntaxKind.FunctionLikeDefineDirectiveTrivia:
                        if (current.Head.GetIdentifier() == id)
                        {
                            directive = (DefineDirectiveTriviaSyntax) current.Head.Node;
                            return DefineState.Defined;
                        }
                        break;

                    case SyntaxKind.UndefDirectiveTrivia:
                        if (current.Head.GetIdentifier() == id)
                        {
                            directive = null;
                            return DefineState.Undefined;
                        }
                        break;


                    case SyntaxKind.ElifDirectiveTrivia:
                    case SyntaxKind.ElseDirectiveTrivia:
                        // Skip directives from previous branches of the same #if.
                        do
                        {
                            current = current.Tail;

                            if (current == null || !current.Any())
                            {
                                directive = null;
                                return DefineState.Unspecified;
                            }
                        }
                        while (!current.Head.Kind.IsIfLikeDirective());

                        break;
                }
            }

            directive = null;
            return DefineState.Unspecified;
        }

        // true if any previous section of the closest #if has its branch taken
        public bool PreviousBranchTaken()
        {
            for (var current = _directives; current != null && current.Any(); current = current.Tail)
            {
                if (current.Head.BranchTaken)
                {
                    return true;
                }
                else if (current.Head.Kind.IsIfLikeDirective())
                {
                    return false;
                }
            }

            return false;
        }

        public bool HasUnfinishedIf()
        {
            var prev = GetPreviousIfElifElse(_directives);
            return prev != null && prev.Any();
        }

        public bool HasPreviousIfOrElif()
        {
            var prev = GetPreviousIfElifElse(_directives);
            return prev != null && prev.Any() && (prev.Head.Kind.IsIfLikeDirective() || prev.Head.Kind == SyntaxKind.ElifDirectiveTrivia);
        }

        public DirectiveStack Add(Directive directive)
        {
            switch (directive.Kind)
            {
                case SyntaxKind.EndIfDirectiveTrivia:
                    var prevIf = GetPreviousIf(_directives);
                    if (prevIf == null || !prevIf.Any())
                    {
                        goto default; // no matching if directive !! leave directive alone
                    }

                    return new DirectiveStack(CompleteIf(directive.Node, _directives, out _));
                default:
                    return new DirectiveStack(new ConsList<Directive>(directive, _directives != null ? _directives : ConsList<Directive>.Empty));
            }
        }

        // removes unfinished if & related directives from stack and leaves active branch directives
        private static ConsList<Directive> CompleteIf(DirectiveTriviaSyntax branchEndSyntax, ConsList<Directive> stack, out bool include)
        {
            // if we get to the top, the default rule is to include anything that follows
            if (!stack.Any())
            {
                include = true;
                return stack;
            }

            // if we reach the #if directive, then we stop unwinding and start
            // rebuilding the stack w/o the #if/#elif/#else/#endif directives
            // only including content from sections that are considered included
            var head = stack.Head;
            if (head.Kind.IsIfLikeDirective())
            {
                include = head.BranchTaken;
                head.BranchEnd = branchEndSyntax;
                return stack.Tail;
            }

            var newStack = CompleteIf(branchEndSyntax, stack.Tail, out include);
            switch (stack.Head.Kind)
            {
                case SyntaxKind.ElifDirectiveTrivia:
                case SyntaxKind.ElseDirectiveTrivia:
                    include = stack.Head.BranchTaken;
                    break;
                default:
                    if (include)
                    {
                        newStack = new ConsList<Directive>(head, newStack);
                    }

                    break;
            }

            return newStack;
        }

        private static ConsList<Directive> GetPreviousIf(ConsList<Directive> directives)
        {
            var current = directives;
            while (current != null && current.Any())
            {
                switch (current.Head.Kind)
                {
                    case SyntaxKind.IfDirectiveTrivia:
                    case SyntaxKind.IfDefDirectiveTrivia:
                    case SyntaxKind.IfNDefDirectiveTrivia:
                        return current;
                }

                current = current.Tail;
            }

            return current;
        }

        private static ConsList<Directive> GetPreviousIfElifElse(ConsList<Directive> directives)
        {
            var current = directives;
            while (current != null && current.Any())
            {
                switch (current.Head.Kind)
                {
                    case SyntaxKind.IfDirectiveTrivia:
                    case SyntaxKind.IfDefDirectiveTrivia:
                    case SyntaxKind.IfNDefDirectiveTrivia:
                    case SyntaxKind.ElifDirectiveTrivia:
                    case SyntaxKind.ElseDirectiveTrivia:
                        return current;
                }

                current = current.Tail;
            }

            return current;
        }
    }
}