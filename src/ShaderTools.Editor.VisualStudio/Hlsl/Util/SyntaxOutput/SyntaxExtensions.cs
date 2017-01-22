using System;
using System.Text;
using ShaderTools.Hlsl.Syntax;

namespace ShaderTools.Editor.VisualStudio.Hlsl.Util.SyntaxOutput
{
    internal static class SyntaxExtensions
    {
        public static string GetDescription(this FunctionDeclarationSyntax syntax, bool includeReturnType, bool includeParameterNames)
        {
            return GetFunctionDescription(syntax.ReturnType, syntax.Name.GetUnqualifiedName().Name, syntax.ParameterList, includeReturnType, includeParameterNames);
        }

        public static string GetDescription(this FunctionDefinitionSyntax syntax, bool includeReturnType, bool includeParameterNames)
        {
            return GetFunctionDescription(syntax.ReturnType, syntax.Name.GetUnqualifiedName().Name, syntax.ParameterList, includeReturnType, includeParameterNames);
        }

        private static string GetFunctionDescription(TypeSyntax returnType, SyntaxToken name, ParameterListSyntax parameterList, bool includeReturnType, bool includeParameterNames)
        {
            var result = new StringBuilder();

            if (includeReturnType)
                result.Append($"{returnType.ToStringIgnoringMacroReferences()} ");

            result.Append(name.GetFullyQualifiedName());
            result.Append("(");

            for (var i = 0; i < parameterList.Parameters.Count; i++)
            {
                var parameter = parameterList.Parameters[i];

                result.Append(parameter.GetDescription(includeParameterNames));

                if (i < parameterList.Parameters.Count - 1)
                    result.Append(", ");
            }

            result.Append(")");

            return result.ToString().Replace(Environment.NewLine, string.Empty);
        }

        public static string GetDescription(this ParameterSyntax parameter, bool includeParameterName)
        {
            var result = new StringBuilder();

            result.Append(parameter.Type.ToStringIgnoringMacroReferences());

            if (includeParameterName)
            {
                result.Append(" ");
                result.Append(parameter.Declarator.Identifier.GetFullyQualifiedName());
            }

            return result.ToString().Replace(Environment.NewLine, string.Empty);
        }

        public static string GetDescription(this VariableDeclarationSyntax declaration, VariableDeclaratorSyntax declarator)
        {
            var result = new StringBuilder();

            result.Append(declaration.Type.ToStringIgnoringMacroReferences());
            result.Append(" ");
            result.Append(declarator.Identifier.GetFullyQualifiedName());

            return result.ToString().Replace(Environment.NewLine, string.Empty);
        }

        public static string GetFullyQualifiedName(this SyntaxToken name)
        {
            if (name.Parent.Kind == SyntaxKind.IdentifierDeclarationName)
                return ((IdentifierDeclarationNameSyntax) name.Parent).GetFullDeclarationName();

            var fullyQualifiedName = GetFullyQualifiedName(name.Parent.Parent);
            if (string.IsNullOrEmpty(fullyQualifiedName))
                return name.Text;

            return fullyQualifiedName + "::" + name.Text;
        }

        public static string GetFullyQualifiedName(this SyntaxNode node)
        {
            var fullName = string.Empty;

            var parent = node;
            while (parent != null)
            {
                switch (parent.Kind)
                {
                    case SyntaxKind.Namespace:
                        var @namespace = (NamespaceSyntax) parent;
                        fullName = @namespace.Name.Text + "::" + fullName;
                        break;
                    case SyntaxKind.ConstantBufferDeclaration:
                        var constantBuffer = (ConstantBufferSyntax) parent;
                        fullName = constantBuffer.Name.Text + "::" + fullName;
                        break;
                    case SyntaxKind.InterfaceType:
                        var interfaceType = (InterfaceTypeSyntax) parent;
                        fullName = interfaceType.Name.Text + "::" + fullName;
                        break;
                    case SyntaxKind.ClassType:
                    case SyntaxKind.StructType:
                        var structType = (StructTypeSyntax) parent;
                        fullName = structType.Name.Text + "::" + fullName;
                        break;
                    case SyntaxKind.IdentifierDeclarationName:
                        return ((IdentifierDeclarationNameSyntax) parent).GetFullDeclarationName();
                }
                parent = parent.Parent;
            }
            return fullName.TrimEnd(':');
        }

        public static string GetFullDeclarationName(this IdentifierDeclarationNameSyntax identifierDeclarationName)
        {
            var result = identifierDeclarationName.Name.Text;

            var parent = identifierDeclarationName.Parent;
            if (parent.Kind == SyntaxKind.QualifiedDeclarationName)
            {
                var qualifiedDeclarationName = (QualifiedDeclarationNameSyntax) parent;
                result = qualifiedDeclarationName.Left.ToStringIgnoringMacroReferences() + "::" + result;
            }

            return result;
        }
    }
}