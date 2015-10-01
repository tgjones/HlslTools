using System.Collections.Generic;

namespace HlslTools.Syntax
{
    public class SwitchSectionSyntax : SyntaxNode
    {
        public readonly List<SwitchLabelSyntax> Labels;
        public readonly List<StatementSyntax> Statements;

        public SwitchSectionSyntax(List<SwitchLabelSyntax> labels, List<StatementSyntax> statements)
            : base(SyntaxKind.SwitchSection)
        {
            RegisterChildNodes(out Labels, labels);
            RegisterChildNodes(out Statements, statements);
        }

        public override void Accept(SyntaxVisitor visitor)
        {
            visitor.VisitSwitchSection(this);
        }

        public override T Accept<T>(SyntaxVisitor<T> visitor)
        {
            return visitor.VisitSwitchSection(this);
        }
    }
}