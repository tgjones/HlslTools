using System.Collections.Generic;
using System.Collections.Immutable;
using HlslTools.Text;

namespace HlslTools.VisualStudio.IntelliSense.SignatureHelp
{
    internal sealed class SignatureHelpModel
    {
        public SignatureHelpModel(TextSpan applicableSpan, IEnumerable<SignatureItem> signatures, SignatureItem signature, int selectedParameter)
        {
            Signatures = signatures.ToImmutableArray();
            ApplicableSpan = applicableSpan;
            Signature = signature;
            SelectedParameter = selectedParameter;
        }

        public TextSpan ApplicableSpan { get; }
        public ImmutableArray<SignatureItem> Signatures { get; }
        public SignatureItem Signature { get; }
        public int SelectedParameter { get; }

        public SignatureHelpModel WithSignature(SignatureItem signatureItem)
        {
            return new SignatureHelpModel(ApplicableSpan, Signatures, signatureItem, SelectedParameter);
        }
    }
}