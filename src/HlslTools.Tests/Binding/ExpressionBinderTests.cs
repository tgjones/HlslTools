using System.Collections.Generic;
using System.Linq;
using HlslTools.Binding;
using HlslTools.Binding.BoundNodes;
using HlslTools.Diagnostics;
using HlslTools.Symbols;
using HlslTools.Syntax;
using NUnit.Framework;

namespace HlslTools.Tests.Binding
{
    [TestFixture]
    public class ExpressionBinderTests
    {
        //[Test, Ignore("Semantic analysis not implemented yet")]
        //public void BindMemberAccess()
        //{
        //    var boundExpression = BindExpression("a.b");

        //    Assert.That(boundExpression.Kind, Is.EqualTo(BoundNodeKind.MemberExpression));
        //    Assert.That(boundExpression.Type, Is.EqualTo(IntrinsicTypes.Int));
        //}

        //private BoundExpression BindExpression(string code)
        //{
        //    var expressionSyntax = SyntaxFactory.ParseExpression(code);
        //    Assert.That(expressionSyntax.GetDiagnostics().Count(), Is.EqualTo(0));

        //    var symbolTable = new ImplementationBinder();
        //    MemberSymbol memberContext = null;

        //    var diagnostics = new List<Diagnostic>();
        //    var expressionBinder = new ExpressionBinder(symbolTable, memberContext, diagnostics);
        //    Assert.That(diagnostics, Has.Count.EqualTo(0));

        //    return expressionBinder.BindExpression(expressionSyntax);
        //}
    }
}