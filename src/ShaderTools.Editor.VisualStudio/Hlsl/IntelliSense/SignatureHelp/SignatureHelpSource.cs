using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    internal sealed class SignatureHelpSource : ISignatureHelpSource
    {
        private readonly ITextBuffer _textBuffer;

        public SignatureHelpSource(ITextBuffer textBuffer)
        {
            _textBuffer = textBuffer;
        }

        public void Dispose()
        {
            
        }

        public void AugmentSignatureHelpSession(ISignatureHelpSession session, IList<ISignature> signatures)
        {
            SignatureHelpManager signatureHelpManager;
            if (!session.Properties.TryGetProperty(typeof(SignatureHelpManager), out signatureHelpManager))
                return;

            var model = signatureHelpManager.Model;
            if (model == null)
                return;

            var snapshot = _textBuffer.CurrentSnapshot;
            var span = model.ApplicableSpan;
            var trackingSpan = snapshot.CreateTrackingSpan(span.Start, span.Length, SpanTrackingMode.EdgeExclusive);

            var signaturesMap = ToSignatures(trackingSpan, model.Signatures, model.SelectedParameter);
            var signatureMapKey = typeof(Dictionary<SignatureItem, ISignature>);
            session.Properties.RemoveProperty(signatureMapKey);
            session.Properties.AddProperty(signatureMapKey, signaturesMap);

            foreach (var signature in model.Signatures)
                signatures.Add(signaturesMap[signature]);
        }

        private Dictionary<SignatureItem, ISignature> ToSignatures(ITrackingSpan applicableSpan, IEnumerable<SignatureItem> signatures, int selectedParameter)
        {
            return signatures.ToDictionary(s => s, s => (ISignature) new Signature(applicableSpan, s, selectedParameter));
        }

        public ISignature GetBestMatch(ISignatureHelpSession session)
        {
            SignatureHelpManager signatureHelpManager;
            if (!session.Properties.TryGetProperty(typeof(SignatureHelpManager), out signatureHelpManager))
                return null;

            Dictionary<SignatureItem, ISignature> signaturesMap;
            if (!session.Properties.TryGetProperty(typeof(Dictionary<SignatureItem, ISignature>), out signaturesMap))
                return null;

            var model = signatureHelpManager.Model;
            if (model == null)
                return null;

            if (model.Signature == null)
                return null;

            return signaturesMap[model.Signature];
        }
    }
}