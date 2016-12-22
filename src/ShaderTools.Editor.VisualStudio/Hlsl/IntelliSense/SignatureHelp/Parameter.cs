using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    internal sealed class Parameter : IParameter
    {
        public Parameter(ISignature signature, string name, string documentation, Span locus)
        {
            Signature = signature;
            Name = name;
            Documentation = documentation;
            Locus = locus;
        }

        public ISignature Signature { get; }
        public string Name { get; }
        public string Documentation { get; }
        public Span Locus { get; }
        public Span PrettyPrintedLocus { get; }
    }
}