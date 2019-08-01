using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using SyntaxGenerator.Model;

namespace SyntaxGenerator.Writer
{
    internal class SourceWriter : AbstractFileWriter
    {
        private SourceWriter(TextWriter writer, Tree tree)
            : base(writer, tree)
        {
        }

        public static void WriteAll(TextWriter writer, Tree tree)
        {
            var sourceWriter = new SourceWriter(writer, tree);
            sourceWriter.WriteFileHeader();
            sourceWriter.WriteSyntax();
            sourceWriter.WriteMain();
        }

        private void WriteFileHeader()
        {
            WriteLine();
            WriteLine("using System;");
            WriteLine("using System.Collections;");
            WriteLine("using System.Collections.Generic;");
            WriteLine("using System.Collections.Immutable;");
            WriteLine("using System.Linq;");
            WriteLine("using System.Threading;");
            WriteLine("using ShaderTools.CodeAnalysis.Diagnostics;");
            WriteLine("using ShaderTools.CodeAnalysis.Syntax;");

            foreach (var us in Tree.Usings)
                WriteLine($"using {us.Namespace};");

            WriteLine();
        }

        private void WriteSyntax()
        {
            WriteLine();
            WriteLine($"namespace {Tree.Namespace}");
            WriteLine("{");
            WriteLine();
            WriteGreenTypes();
            WriteLine("}");
        }

        private void WriteMain()
        {
            WriteLine();
            WriteLine($"namespace {Tree.Namespace}");
            WriteLine("{");
            WriteLine();
            WriteVisitors();
            WriteRewriter();
            //WriteFactories();
            WriteLine("}");
        }

        private void WriteGreenTypes()
        {
            var nodes = Tree.Types.Where(n => !(n is PredefinedNode)).ToList();
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i];
                WriteLine();
                this.WriteGreenType(node);
            }
        }

        private void WriteGreenType(TreeType node)
        {
            WriteComment(node.TypeComment, "  ");

            if (node is AbstractNode)
            {
                AbstractNode nd = (AbstractNode) node;
                WriteLine("  public abstract partial class {0} : {1}", node.Name, node.Base);
                WriteLine("  {");

                var concreteFields = nd.Fields.Where(f => !IsAbstract(f)).ToList();
                var abstractFields = nd.Fields.Where(f => IsAbstract(f)).ToList();

                foreach (var field in concreteFields)
                {
                    var type = GetFieldType(field);
                    WriteLine("    private readonly {0} {1};", type, CamelCase(field.Name));
                }

                var fieldArgs = string.Empty;
                if (concreteFields.Any())
                {
                    fieldArgs = concreteFields.Aggregate("", (str, a) => str + $", {a.Type} {CamelCase(a.Name)}");
                }
                    
                // ctor with diagnostics
                WriteLine("    protected {0}(SyntaxKind kind{1}, IEnumerable<Diagnostic> diagnostics)", node.Name, fieldArgs);
                WriteLine("      : base(kind, diagnostics)");
                WriteLine("    {");
                var valueFields = concreteFields.Where(n => !IsNodeOrNodeList(n.Type)).ToList();
                var nodeFields = concreteFields.Where(n => IsNodeOrNodeList(n.Type)).ToList();
                WriteCtorBody(valueFields, nodeFields);

                WriteLine("    }");

                // ctor without diagnostics
                WriteLine("    protected {0}(SyntaxKind kind{1})", node.Name, fieldArgs);
                WriteLine("      : base(kind)");
                WriteLine("    {");
                WriteCtorBody(valueFields, nodeFields);

                WriteLine("    }");

                foreach (var field in concreteFields)
                {
                    WriteLine();
                    WriteComment(field.PropertyComment, "    ");

                    WriteLine("    public {0}{1} {2} => {3};",
                        (IsNew(field) ? "new " : ""), field.Type, field.Name, CamelCase(field.Name));
                }
                foreach (var field in abstractFields)
                {
                    WriteLine();
                    WriteComment(field.PropertyComment, "    ");

                    WriteLine("    public abstract {0}{1} {2} {{ get; }}",
                        (IsNew(field) ? "new " : ""), field.Type, field.Name);
                }

                WriteLine("  }");
            }
            else if (node is Node)
            {
                Node nd = (Node) node;

                var baseFields = GetBaseFields(nd);
                var hasDerivedTypes = nd.Fields.Any(IsDerived);

                WriteLine("  public sealed partial class {0} : {1}", node.Name, node.Base);
                WriteLine("  {");

                var valueFields = nd.Fields.Where(n => !IsNodeOrNodeList(n.Type)).ToList();
                var nodeFields = nd.Fields.Where(n => IsNodeOrNodeList(n.Type)).ToList();

                for (int i = 0, n = nodeFields.Count; i < n; i++)
                {
                    var field = nodeFields[i];
                    var type = GetFieldType(field);
                    WriteLine("    private readonly {0} {1};", type, CamelCase(field.Name));
                }

                for (int i = 0, n = valueFields.Count; i < n; i++)
                {
                    var field = valueFields[i];
                    WriteLine("    private readonly {0} {1};", field.Type, CamelCase(field.Name));
                }

                var ctorAccess = hasDerivedTypes ? "private" : "public";

                // write constructor with diagnostics
                WriteLine();
                if (HasOneKind(nd))
                    Write("    {0} {1}(", ctorAccess, node.Name);
                else
                    Write("    {0} {1}(SyntaxKind kind, ", ctorAccess, node.Name);

                if (baseFields.Any())
                    Write(baseFields.Aggregate("", (str, a) => str + $"{a.Type} {CamelCase(a.Name)}, "));
                WriteGreenNodeConstructorArgs(nodeFields, valueFields);

                var baseFieldsStr = (baseFields.Any() ? ", " : string.Empty) + string.Join(", ", baseFields.Select(a => CamelCase(a.Name)));
                WriteLine(", IEnumerable<Diagnostic> diagnostics)");
                if (HasOneKind(nd))
                    WriteLine("        : base(SyntaxKind.{0}{1}, diagnostics)", nd.Kinds[0].Name, baseFieldsStr);
                else
                    WriteLine("        : base(kind{0}, diagnostics)", baseFieldsStr);
                WriteLine("    {");
                WriteCtorBody(valueFields, nodeFields);
                WriteLine("    }");
                WriteLine();

                // write constructor without diagnostics
                WriteLine();
                if (HasOneKind(nd))
                    Write("    {0} {1}(", ctorAccess, node.Name);
                else
                    Write("    {0} {1}(SyntaxKind kind, ", ctorAccess, node.Name);

                if (baseFields.Any())
                    Write(baseFields.Aggregate("", (str, a) => str + $"{a.Type} {CamelCase(a.Name)}, "));
                WriteGreenNodeConstructorArgs(nodeFields, valueFields);

                WriteLine(")");
                if (HasOneKind(nd))
                    WriteLine("        : base(SyntaxKind.{0}{1})", nd.Kinds[0].Name, baseFieldsStr);
                else
                    WriteLine("        : base(kind{0})", baseFieldsStr);
                WriteLine("    {");
                WriteCtorBody(valueFields, nodeFields);
                WriteLine("    }");
                WriteLine();

                // property accessors
                for (int i = 0, n = nodeFields.Count; i < n; i++)
                {
                    var field = nodeFields[i];
                    WriteComment(field.PropertyComment, "    ");
                    WriteLine("    public {0}{1} {2} {{ get {{ return this.{3}; }} }}",
                        OverrideOrNewModifier(field), field.Type, field.Name, CamelCase(field.Name)
                        );

                    // additional getters
                    foreach (var getter in field.Getters)
                    {
                        WriteLine("    public {0}{1} {2} {{ get {{ return this.{3}; }} }}",
                            OverrideOrNewModifier(getter), field.Type, getter.Name, CamelCase(field.Name)
                            );
                    }
                }

                for (int i = 0, n = valueFields.Count; i < n; i++)
                {
                    var field = valueFields[i];
                    WriteComment(field.PropertyComment, "    ");
                    WriteLine("    public {0}{1} {2} {{ get {{ return this.{3}; }} }}",
                        OverrideOrNewModifier(field), field.Type, field.Name, CamelCase(field.Name)
                        );

                    // additional getters
                    foreach (var getter in field.Getters)
                    {
                        WriteLine("    public {0}{1} {2} {{ get {{ return this.{3}; }} }}",
                            OverrideOrNewModifier(getter), field.Type, getter.Name, CamelCase(field.Name)
                            );
                    }
                }

                this.WriteGreenAcceptMethods(nd);
                this.WriteGreenUpdateMethod(nd);
                this.WriteRedSetters(nd);
                this.WriteSetDiagnostics(nd);

                WriteLine("  }");
            }
        }

        private void WriteGreenNodeConstructorArgs(List<Field> nodeFields, List<Field> valueFields)
        {
            var first = true;
            for (int i = 0, n = nodeFields.Count; i < n; i++)
            {
                var field = nodeFields[i];
                string type = GetFieldType(field);

                if (!first)
                {
                    Write(", ");
                }

                first = false;
                Write("{0} {1}", type, CamelCase(field.Name));
            }

            for (int i = 0, n = valueFields.Count; i < n; i++)
            {
                var field = valueFields[i];
                if (!first)
                {
                    Write(", ");
                }

                first = false;
                Write("{0} {1}", field.Type, CamelCase(field.Name));
            }
        }

        private void WriteCtorBody(List<Field> valueFields, List<Field> nodeFields)
        {
            // constructor body
            for (int i = 0, n = nodeFields.Count; i < n; i++)
            {
                var field = nodeFields[i];
                if (IsAnyList(field.Type) || IsOptional(field))
                {
                    WriteLine("        if ({0} != null)", CamelCase(field.Name));
                    WriteLine("        {");
                    WriteLine("            RegisterChildNodes(out this.{0}, {0});", CamelCase(field.Name));
                    WriteLine("        }");
                }
                else
                {
                    WriteLine("        RegisterChildNode(out this.{0}, {0});", CamelCase(field.Name));
                }

            }

            for (int i = 0, n = valueFields.Count; i < n; i++)
            {
                var field = valueFields[i];
                WriteLine("        this.{0} = {0};", CamelCase(field.Name));
            }
        }

        private void WriteSetDiagnostics(Node node)
        {
            WriteLine();
            WriteLine($"    public override {Tree.Root} SetDiagnostics(ImmutableArray<Diagnostic> diagnostics)");
            WriteLine("    {");

            Write("         return new {0}(", node.Name);

            var first = true;
            if (!HasOneKind(node))
            {
                Write("this.Kind");
                first = false;
            }
            var baseFields = GetBaseFields(node);
            foreach (var arg in baseFields)
            {
                if (!first)
                    Write(", ");
                first = false;
                Write("this." + arg.Name);
            }

            for (int f = 0; f < node.Fields.Count; f++)
            {
                var field = node.Fields[f];
                if (!first)
                    Write(", ");
                first = false;
                Write("this.{0}", CamelCase(field.Name));
            }
            WriteLine(", diagnostics);");
            WriteLine("    }");
        }

        private void WriteGreenAcceptMethods(Node node)
        {
            //WriteLine();
            //WriteLine("    public override T Accept<TArgument, T>(SyntaxVisitor<TArgument, T> visitor, TArgument argument)");
            //WriteLine("    {");
            //WriteLine("        return visitor.Visit{0}(this, argument);", StripPost(node.Name, "Syntax"));
            //WriteLine("    }");
            WriteLine();
            WriteLine($"    public override T Accept<T>(SyntaxVisitor<T> visitor)");
            WriteLine("    {");
            WriteLine("        return visitor.Visit{0}(this);", StripPost(node.Name, "Syntax"));
            WriteLine("    }");
            WriteLine();
            WriteLine($"    public override void Accept(SyntaxVisitor visitor)");
            WriteLine("    {");
            WriteLine("        visitor.Visit{0}(this);", StripPost(node.Name, "Syntax"));
            WriteLine("    }");
        }

        private void WriteVisitors()
        {
            //WriteGreenVisitor(true, true);
            //WriteLine();
            WriteGreenVisitor(false, true);
            WriteLine();
            WriteGreenVisitor(false, false);
        }

        private void WriteGreenVisitor(bool withArgument, bool withResult)
        {
            var nodes = Tree.Types.Where(n => !(n is PredefinedNode)).ToList();

            WriteLine();
            WriteLine($"  public abstract partial class SyntaxVisitor" + (withResult ? "<" + (withArgument ? "TArgument, " : "") + "T>" : ""));
            WriteLine("  {");
            int nWritten = 0;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i] as Node;
                if (node != null)
                {
                    if (nWritten > 0)
                        WriteLine();
                    nWritten++;
                    WriteLine("    public virtual " + (withResult ? "T" : "void") + " Visit{0}({1} node{2})", StripPost(node.Name, "Syntax"), node.Name, withArgument ? ", TArgument argument" : "");
                    WriteLine("    {");
                    WriteLine("      " + (withResult ? "return " : "") + "this.DefaultVisit(node{0});", withArgument ? ", argument" : "");
                    WriteLine("    }");
                }
            }
            WriteLine("  }");
        }

        private void WriteGreenUpdateMethod(Node node)
        {
            WriteLine();
            Write("    public {0} Update(", node.Name);

            // parameters
            var first = true;

            for (int f = 0; f < node.Fields.Count; f++)
            {
                var field = node.Fields[f];

                if (IsDerived(field))
                    continue;

                if (!first)
                    Write(", ");

                first = false;

                var type =
                    //field.Type == "SyntaxNodeOrTokenList" ? "Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<CSharpSyntaxNode>" :
                    //field.Type == "SyntaxTokenList" ? "Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken>" :
                    //IsNodeList(field.Type) ? "Microsoft.CodeAnalysis.Syntax.InternalSyntax." + field.Type :
                    //IsSeparatedNodeList(field.Type) ? "Microsoft.CodeAnalysis.Syntax.InternalSyntax." + field.Type :
                    field.Type;

                Write("{0} {1}", type, CamelCase(field.Name));
            }
            WriteLine(")");
            WriteLine("    {");

            Write("        if (");
            int nCompared = 0;
            for (int f = 0; f < node.Fields.Count; f++)
            {
                var field = node.Fields[f];
                if (IsDerivedOrListOfDerived("SyntaxNode", field.Type) || IsDerivedOrListOfDerived("SyntaxToken", field.Type) || field.Type == "SyntaxNodeOrTokenList")
                {
                    if (nCompared > 0)
                        Write(" || ");
                    Write("{0} != this.{1}", CamelCase(field.Name), field.Name);
                    nCompared++;
                }
            }
            if (nCompared > 0)
            {
                WriteLine(")");
                WriteLine("        {");
                Write("            var newNode = new {0}(", node.Name);
                if (!HasOneKind(node))
                {
                    Write("this.Kind, ");
                }

                first = true;
                var baseFields = GetBaseFields(node);
                foreach (var field in baseFields)
                {
                    if (!first)
                        Write(", ");
                    first = false;
                    Write("this." + field.Name);
                }

                foreach (var field in node.Fields)
                {
                    if (!first)
                        Write(", ");
                    first = false;
                    Write(CamelCase(field.Name));
                }
                WriteLine(");");
                WriteLine("            var diags = this.GetDiagnostics();");
                WriteLine("            if (diags != null && diags.Any())");
                WriteLine("               newNode = newNode.WithDiagnostics(diags);");
                WriteLine("            return newNode;");
                WriteLine("        }");
            }

            WriteLine();
            WriteLine("        return this;");
            WriteLine("    }");
        }

        private void WriteCtorArgList(Node nd, List<Field> valueFields, List<Field> nodeFields)
        {
            if (nd.Kinds.Count == 1)
            {
                Write("SyntaxKind.");
                Write(nd.Kinds[0].Name);
            }
            else
            {
                Write("kind");
            }
            for (int i = 0, n = nodeFields.Count; i < n; i++)
            {
                var field = nodeFields[i];
                Write(", ");
                if (field.Type == "SyntaxList<SyntaxToken>" || IsAnyList(field.Type))
                {
                    Write("{0}.Node", CamelCase(field.Name));
                }
                else
                {
                    Write(CamelCase(field.Name));
                }
            }
            // values are at end
            for (int i = 0, n = valueFields.Count; i < n; i++)
            {
                var field = valueFields[i];
                Write(", ");
                Write(CamelCase(field.Name));
            }
        }

        private void WriteRedSetters(Node node)
        {
            for (int f = 0; f < node.Fields.Count; f++)
            {
                var field = node.Fields[f];
                if (IsDerived(field))
                    continue;

                var type = this.GetRedPropertyType(field);

                WriteLine();
                WriteLine("    {0} {1} With{2}({3} {4})", "public", node.Name, StripPost(field.Name, "Opt"), type, CamelCase(field.Name));
                WriteLine("    {");

                // call update inside each setter
                Write("        return this.Update(");

                var first = true;
                for (int f2 = 0; f2 < node.Fields.Count; f2++)
                {
                    var field2 = node.Fields[f2];
                    if (IsDerived(field2))
                        continue;

                    if (!first)
                        Write(", ");

                    first = false;

                    if (field2 == field)
                    {
                        this.Write("{0}", CamelCase(field2.Name));
                    }
                    else
                    {
                        this.Write("this.{0}", field2.Name);
                    }
                }
                WriteLine(");");

                WriteLine("    }");
            }
        }

        private void WriteRedListHelperMethods(Node node)
        {
            for (int f = 0; f < node.Fields.Count; f++)
            {
                var field = node.Fields[f];
                if (IsAnyList(field.Type))
                {
                    // write list helper methods for list properties
                    WriteRedListHelperMethods(node, field);
                }
                else
                {
                    Node referencedNode = GetNode(field.Type);
                    if (referencedNode != null && (!IsOptional(field) || RequiredFactoryArgumentCount(referencedNode) == 0))
                    {
                        // look for list members...
                        for (int rf = 0; rf < referencedNode.Fields.Count; rf++)
                        {
                            var referencedNodeField = referencedNode.Fields[rf];
                            if (IsAnyList(referencedNodeField.Type))
                            {
                                WriteRedNestedListHelperMethods(node, field, referencedNode, referencedNodeField);
                            }
                        }
                    }
                }
            }
        }

        private void WriteRedListHelperMethods(Node node, Field field)
        {
            var argType = GetElementType(field.Type);
            WriteLine();
            WriteLine("    public {0} Add{1}(params {2}[] items)", node.Name, field.Name, argType);
            WriteLine("    {");
            WriteLine("        return this.With{0}(this.{1}.AddRange(items));", StripPost(field.Name, "Opt"), field.Name);
            WriteLine("    }");
        }

        private void WriteRedNestedListHelperMethods(Node node, Field field, Node referencedNode, Field referencedNodeField)
        {
            var argType = GetElementType(referencedNodeField.Type);

            // AddBaseListTypes
            WriteLine();
            WriteLine("    public {0} Add{1}{2}(params {3}[] items)", node.Name, StripPost(field.Name, "Opt"), referencedNodeField.Name, argType);
            WriteLine("    {");

            if (IsOptional(field))
            {
                var factoryName = StripPost(referencedNode.Name, "Syntax");
                var varName = StripPost(CamelCase(field.Name), "Opt");
                WriteLine("        var {0} = this.{1} ?? SyntaxFactory.{2}();", varName, field.Name, factoryName);
                WriteLine("        return this.With{0}({1}.With{2}({1}.{3}.AddRange(items)));", StripPost(field.Name, "Opt"), varName, StripPost(referencedNodeField.Name, "Opt"), referencedNodeField.Name);
            }
            else
            {
                WriteLine("        return this.With{0}(this.{1}.With{2}(this.{1}.{3}.AddRange(items)));", StripPost(field.Name, "Opt"), field.Name, StripPost(referencedNodeField.Name, "Opt"), referencedNodeField.Name);
            }

            WriteLine("    }");
        }

        private void WriteRewriter()
        {
            var nodes = Tree.Types.Where(n => !(n is PredefinedNode)).ToList();

            WriteLine();
            WriteLine($"  public abstract partial class SyntaxRewriter : SyntaxVisitor<SyntaxNode>");
            WriteLine("  {");
            int nWritten = 0;
            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i] as Node;
                if (node != null)
                {
                    var nodeFields = node.Fields.Where(nd => IsNodeOrNodeList(nd.Type)).ToList();

                    if (nWritten > 0)
                        WriteLine();
                    nWritten++;
                    WriteLine("    public override SyntaxNode Visit{0}({1} node)", StripPost(node.Name, "Syntax"), node.Name);
                    WriteLine("    {");
                    for (int f = 0; f < nodeFields.Count; f++)
                    {
                        var field = nodeFields[f];
                        if (IsAnyList(field.Type))
                        {
                            WriteLine("      var {0} = this.VisitList(node.{1});", CamelCase(field.Name), field.Name);
                        }
                        else if (field.Type == "SyntaxToken")
                        {
                            WriteLine("      var {0} = this.VisitToken(node.{1});", CamelCase(field.Name), field.Name);
                        }
                        else
                        {
                            WriteLine("      var {0} = ({1})this.Visit(node.{2});", CamelCase(field.Name), field.Type, field.Name);
                        }
                    }
                    if (nodeFields.Count > 0)
                    {
                        Write("      return node.Update(");
                        var first = true;
                        for (int f = 0; f < node.Fields.Count; f++)
                        {
                            var field = node.Fields[f];
                            if (IsDerived(field))
                                continue;

                            if (!first)
                                Write(", ");

                            first = false;

                            if (IsNodeOrNodeList(field.Type))
                            {
                                Write(CamelCase(field.Name));
                            }
                            else
                            {
                                Write("node.{0}", field.Name);
                            }
                        }
                        WriteLine(");");
                    }
                    else
                    {
                        WriteLine("      return node;");
                    }
                    WriteLine("    }");
                }
            }
            WriteLine("  }");
        }

        private void WriteFactories()
        {
            var nodes = Tree.Types.Where(n => !(n is PredefinedNode) && !(n is AbstractNode)).OfType<Node>().ToList();
            WriteLine();
            WriteLine("  public static partial class SyntaxFactory");
            WriteLine("  {");

            for (int i = 0, n = nodes.Count; i < n; i++)
            {
                var node = nodes[i];
                this.WriteRedFactory(node);
                this.WriteRedFactoryWithNoAutoCreatableTokens(node);
                this.WriteRedMinimalFactory(node);
                this.WriteRedMinimalFactory(node, withStringNames: true);
                this.WriteKindConverters(node);
            }

            WriteLine("  }");
        }

        protected bool CanBeAutoCreated(Node node, Field field)
        {
            return IsAutoCreatableToken(node, field) || IsAutoCreatableNode(node, field);
        }

        private bool IsAutoCreatableToken(Node node, Field field)
        {
            return field.Type == "SyntaxToken"
                && field.Kinds != null
                && ((field.Kinds.Count == 1 && field.Kinds[0].Name != "IdentifierToken" && !field.Kinds[0].Name.EndsWith("LiteralToken", StringComparison.Ordinal)) || (field.Kinds.Count > 1 && field.Kinds.Count == node.Kinds.Count));
        }

        private bool IsAutoCreatableNode(Node node, Field field)
        {
            var referencedNode = GetNode(field.Type);
            return (referencedNode != null && RequiredFactoryArgumentCount(referencedNode) == 0);
        }

        private bool IsRequiredFactoryField(Node node, Field field)
        {
            return (!IsOptional(field) && !IsAnyList(field.Type) && !CanBeAutoCreated(node, field)) || IsValueField(field);
        }

        private bool IsValueField(Field field)
        {
            return !IsNodeOrNodeList(field.Type);
        }

        private int RequiredFactoryArgumentCount(Node nd, bool includeKind = true)
        {
            int count = 0;

            // kind must be specified in factory
            if (nd.Kinds.Count > 1 && includeKind)
            {
                count++;
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];
                if (IsRequiredFactoryField(nd, field))
                {
                    count++;
                }
            }

            return count;
        }

        private int OptionalFactoryArgumentCount(Node nd)
        {
            int count = 0;
            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];
                if (IsOptional(field) || CanBeAutoCreated(nd, field) || IsAnyList(field.Type))
                {
                    count++;
                }
            }

            return count;
        }

        // full factory signature with nothing optional
        private void WriteRedFactory(Node nd)
        {
            this.WriteLine();

            var valueFields = nd.Fields.Where(n => IsValueField(n)).ToList();
            var nodeFields = nd.Fields.Where(n => !IsValueField(n)).ToList();

            WriteComment(string.Format("<summary>Creates a new {0} instance.</summary>", nd.Name), "    ");

            Write("    {0} static {1} {2}(", "public", nd.Name, StripPost(nd.Name, "Syntax"));
            WriteRedFactoryParameters(nd);

            WriteLine(")");
            WriteLine("    {");

            // validate kinds
            if (nd.Kinds.Count > 1)
            {
                WriteLine("      switch (kind)");
                WriteLine("      {");
                foreach (var kind in nd.Kinds)
                {
                    WriteLine("        case SyntaxKind.{0}:", kind.Name);
                }
                WriteLine("          break;");
                WriteLine("        default:");
                WriteLine("          throw new ArgumentException(\"kind\");");
                WriteLine("      }");
            }

            // validate parameters
            for (int i = 0, n = nodeFields.Count; i < n; i++)
            {
                var field = nodeFields[i];
                var pname = CamelCase(field.Name);

                if (field.Type == "SyntaxToken")
                {
                    if (field.Kinds != null && field.Kinds.Count > 0)
                    {
                        WriteLine("      switch ({0}.Kind())", pname);
                        WriteLine("      {");
                        foreach (var kind in field.Kinds)
                        {
                            WriteLine("        case SyntaxKind.{0}:", kind.Name);
                        }
                        if (IsOptional(field))
                        {
                            WriteLine("        case SyntaxKind.None:");
                        }
                        WriteLine("          break;");
                        WriteLine("        default:");
                        WriteLine("          throw new ArgumentException(\"{0}\");", pname);
                        WriteLine("      }");
                    }
                }
                else if (!IsAnyList(field.Type) && !IsOptional(field))
                {
                    WriteLine("      if ({0} == null)", CamelCase(field.Name));
                    WriteLine("        throw new ArgumentNullException(nameof({0}));", CamelCase(field.Name));
                }
            }

            //return new IdentifierNameSyntax(SyntaxKind.IdentifierName, identifier);
            Write("      return new {0}(", nd.Name);
            WriteCtorArgList(nd, valueFields, nodeFields);
            WriteLine(");");
            
            WriteLine("    }");

            this.WriteLine();
        }

        private void WriteRedFactoryParameters(Node nd)
        {
            if (nd.Kinds.Count > 1)
            {
                Write("SyntaxKind kind, ");
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];
                if (i > 0)
                    Write(", ");
                var type = this.GetRedPropertyType(field);

                Write("{0} {1}", type, CamelCase(field.Name));
            }
        }

        private string GetRedPropertyType(Field field)
        {
            if (field.Type == "SyntaxList<SyntaxToken>")
                return "SyntaxTokenList";
            return field.Type;
        }

        private string GetDefaultValue(Node nd, Field field)
        {
            System.Diagnostics.Debug.Assert(!IsRequiredFactoryField(nd, field));

            if (IsOptional(field) || IsAnyList(field.Type))
            {
                return string.Format("default({0})", GetRedPropertyType(field));
            }
            else if (field.Type == "SyntaxToken")
            {
                // auto construct token?
                if (field.Kinds.Count == 1)
                {
                    return string.Format("SyntaxFactory.Token(SyntaxKind.{0})", field.Kinds[0].Name);
                }
                else
                {
                    return string.Format("SyntaxFactory.Token(Get{0}{1}Kind(kind))", StripPost(nd.Name, "Syntax"), StripPost(field.Name, "Opt"));
                }
            }
            else
            {
                var referencedNode = GetNode(field.Type);
                return string.Format("SyntaxFactory.{0}()", StripPost(referencedNode.Name, "Syntax"));
            }
        }

        // Writes Get<Property>Kind() methods for converting between node kind and member token kinds...
        private void WriteKindConverters(Node nd)
        {
            for (int f = 0; f < nd.Fields.Count; f++)
            {
                var field = nd.Fields[f];

                if (field.Type == "SyntaxToken" && CanBeAutoCreated(nd, field) && field.Kinds.Count > 1)
                {
                    WriteLine();
                    WriteLine("    private static SyntaxKind Get{0}{1}Kind(SyntaxKind kind)", StripPost(nd.Name, "Syntax"), StripPost(field.Name, "Opt"));
                    WriteLine("    {");

                    WriteLine("      switch (kind)");
                    WriteLine("      {");

                    for (int k = 0; k < field.Kinds.Count; k++)
                    {
                        var nKind = nd.Kinds[k];
                        var pKind = field.Kinds[k];
                        WriteLine("        case SyntaxKind.{0}:", nKind.Name);
                        WriteLine("          return SyntaxKind.{0};", pKind.Name);
                    }

                    WriteLine("        default:");
                    WriteLine("          throw new ArgumentOutOfRangeException();");
                    WriteLine("      }");
                    WriteLine("    }");
                }
            }
        }

        private IEnumerable<Field> DetermineRedFactoryWithNoAutoCreatableTokenFields(Node nd)
        {
            return nd.Fields.Where(f => !IsAutoCreatableToken(nd, f));
        }

        // creates a factory without auto-creatable token arguments
        private void WriteRedFactoryWithNoAutoCreatableTokens(Node nd)
        {
            var nAutoCreatableTokens = nd.Fields.Count(f => IsAutoCreatableToken(nd, f));
            if (nAutoCreatableTokens == 0)
                return; // already handled by general factory

            var factoryWithNoAutoCreatableTokenFields = new HashSet<Field>(DetermineRedFactoryWithNoAutoCreatableTokenFields(nd));
            var minimalFactoryFields = DetermineMinimalFactoryFields(nd);
            if (minimalFactoryFields != null && factoryWithNoAutoCreatableTokenFields.SetEquals(minimalFactoryFields))
            {
                return; // will be handled in minimal factory case
            }

            this.WriteLine();

            WriteComment(string.Format("<summary>Creates a new {0} instance.</summary>", nd.Name), "    ");
            Write("    {0} static {1} {2}(", "public", nd.Name, StripPost(nd.Name, "Syntax"));

            bool hasPreviousParameter = false;
            if (nd.Kinds.Count > 1)
            {
                Write("SyntaxKind kind");
                hasPreviousParameter = true;
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];

                if (factoryWithNoAutoCreatableTokenFields.Contains(field))
                {
                    if (hasPreviousParameter)
                        Write(", ");

                    Write("{0} {1}", GetRedPropertyType(field), CamelCase(field.Name));

                    hasPreviousParameter = true;
                }
            }
            WriteLine(")");

            WriteLine("    {");

            Write("      return SyntaxFactory.{0}(", StripPost(nd.Name, "Syntax"));

            bool hasPreviousArgument = false;
            if (nd.Kinds.Count > 1)
            {
                Write("kind");
                hasPreviousArgument = true;
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];

                if (hasPreviousArgument)
                    Write(", ");

                if (factoryWithNoAutoCreatableTokenFields.Contains(field))
                {
                    // pass supplied parameter on to general factory
                    Write("{0}", CamelCase(field.Name));
                }
                else
                {
                    // pass an auto-created token to the general factory
                    Write("{0}", GetDefaultValue(nd, field));
                }

                hasPreviousArgument = true;
            }

            WriteLine(");");

            WriteLine("    }");
        }

        private Field DetermineMinimalOptionalField(Node nd)
        {
            // first if there is a single list, then choose the list because it would not have been optional
            int listCount = nd.Fields.Count(f => IsAnyNodeList(f.Type));
            if (listCount == 1)
            {
                return nd.Fields.First(f => IsAnyNodeList(f.Type));
            }
            else
            {
                // otherwise, if there is a single optional node, use that..
                int nodeCount = nd.Fields.Count(f => IsNode(f.Type) && f.Type != "SyntaxToken");
                if (nodeCount == 1)
                {
                    return nd.Fields.First(f => IsNode(f.Type) && f.Type != "SyntaxToken");
                }
                else
                {
                    return null;
                }
            }
        }

        private IEnumerable<Field> DetermineMinimalFactoryFields(Node nd)
        {
            // special case to allow a single optional argument if there would have been no arguments
            // and we can determine a best single argument.
            Field allowOptionalField = null;

            var optionalCount = OptionalFactoryArgumentCount(nd);
            if (optionalCount == 0)
            {
                return null; // no fields...
            }

            var requiredCount = RequiredFactoryArgumentCount(nd, includeKind: false);
            if (requiredCount == 0 && optionalCount > 1)
            {
                allowOptionalField = DetermineMinimalOptionalField(nd);
            }

            return nd.Fields.Where(f => IsRequiredFactoryField(nd, f) || allowOptionalField == f);
        }

        // creates a factory with only the required arguments (everything else is defaulted)
        private void WriteRedMinimalFactory(Node nd, bool withStringNames = false)
        {
            var optionalCount = OptionalFactoryArgumentCount(nd);
            if (optionalCount == 0)
                return; // already handled w/ general factory method

            var minimalFactoryfields = new HashSet<Field>(DetermineMinimalFactoryFields(nd));

            if (withStringNames && minimalFactoryfields.Count(f => IsRequiredFactoryField(nd, f) && CanAutoConvertFromString(f)) == 0)
                return; // no string-name overload necessary

            this.WriteLine();

            WriteComment(string.Format("<summary>Creates a new {0} instance.</summary>", nd.Name), "    ");
            Write("    {0} static {1} {2}(", "public", nd.Name, StripPost(nd.Name, "Syntax"));

            bool hasPreviousParameter = false;
            if (nd.Kinds.Count > 1)
            {
                Write("SyntaxKind kind");
                hasPreviousParameter = true;
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];

                if (minimalFactoryfields.Contains(field))
                {
                    var type = GetRedPropertyType(field);

                    if (IsRequiredFactoryField(nd, field))
                    {
                        if (hasPreviousParameter)
                            Write(", ");

                        if (withStringNames && CanAutoConvertFromString(field))
                        {
                            type = "string";
                        }

                        Write("{0} {1}", type, CamelCase(field.Name));

                        hasPreviousParameter = true;
                    }
                    else
                    {
                        if (hasPreviousParameter)
                            Write(", ");

                        Write("{0} {1} = default({0})", type, CamelCase(field.Name));

                        hasPreviousParameter = true;
                    }
                }
            }
            WriteLine(")");

            WriteLine("    {");

            Write("      return SyntaxFactory.{0}(", StripPost(nd.Name, "Syntax"));

            bool hasPreviousArgument = false;
            if (nd.Kinds.Count > 1)
            {
                Write("kind");
                hasPreviousArgument = true;
            }

            for (int i = 0, n = nd.Fields.Count; i < n; i++)
            {
                var field = nd.Fields[i];

                if (hasPreviousArgument)
                    Write(", ");

                if (minimalFactoryfields.Contains(field))
                {
                    if (IsRequiredFactoryField(nd, field))
                    {
                        if (withStringNames && CanAutoConvertFromString(field))
                        {
                            Write("{0}({1})", GetStringConverterMethod(field), CamelCase(field.Name));
                        }
                        else
                        {
                            Write("{0}", CamelCase(field.Name));
                        }
                    }
                    else
                    {
                        if (IsOptional(field) || IsAnyList(field.Type))
                        {
                            Write("{0}", CamelCase(field.Name));
                        }
                        else
                        {
                            Write("{0} ?? {1}", CamelCase(field.Name), GetDefaultValue(nd, field));
                        }
                    }
                }
                else
                {
                    var defaultValue = GetDefaultValue(nd, field);
                    Write(defaultValue);
                }

                hasPreviousArgument = true;
            }

            WriteLine(");");

            WriteLine("    }");
        }

        private bool CanAutoConvertFromString(Field field)
        {
            return IsIdentifierToken(field) || IsIdentifierNameSyntax(field);
        }

        private bool IsIdentifierToken(Field field)
        {
            return field.Type == "SyntaxToken" && field.Kinds != null && field.Kinds.Count == 1 && field.Kinds[0].Name == "IdentifierToken";
        }

        private bool IsIdentifierNameSyntax(Field field)
        {
            return field.Type == "IdentifierNameSyntax";
        }

        private string GetStringConverterMethod(Field field)
        {
            if (IsIdentifierToken(field))
            {
                return "SyntaxFactory.Identifier";
            }
            else if (IsIdentifierNameSyntax(field))
            {
                return "SyntaxFactory.IdentifierName";
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Anything inside a &lt;Comment&gt; tag gets written out (escaping untouched) as the
        /// XML doc comment.  Line breaks will be preserved.
        /// </summary>
        private void WriteComment(string comment, string indent)
        {
            if (comment != null)
            {
                var lines = comment.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                {
                    WriteLine("{0}/// {1}", indent, line.TrimStart());
                }
            }
        }

        /// <summary>
        /// Anything inside a &lt;Comment&gt; tag gets written out (escaping untouched) as the
        /// XML doc comment.  Line breaks will be preserved.
        /// </summary>
        private void WriteComment(Comment comment, string indent)
        {
            if (comment != null)
            {
                foreach (XmlElement element in comment.Body)
                {
                    string[] lines = element.OuterXml.Split(new string[] { "\r", "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string line in lines.Where(l => !string.IsNullOrWhiteSpace(l)))
                    {
                        WriteLine("{0}/// {1}", indent, line.TrimStart());
                    }
                }
            }
        }

        private List<Field> GetBaseFields(Node nd)
        {
            List<Field> baseFields = new List<Field>(0);
            var pn = GetTreeType(nd.Base) as AbstractNode;
            if (pn != null)
            {
                if (pn.Fields != null && pn.Fields.Count > 0)
                    baseFields = pn.Fields.Where(f => !IsAbstract(f)).ToList();
            }

            return baseFields;
        }
    }
}
