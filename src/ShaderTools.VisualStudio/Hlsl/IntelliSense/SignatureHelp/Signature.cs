using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;

namespace ShaderTools.VisualStudio.Hlsl.IntelliSense.SignatureHelp
{
    internal sealed class Signature : ISignature
    {
        private IParameter _currentParameter;

        internal Signature(ITrackingSpan applicableSpan, SignatureItem signatureItem, int selectedParameter)
        {
            var parameters = signatureItem.Parameters.Select(CreateParameter).OfType<IParameter>().ToImmutableArray();

            ApplicableToSpan = applicableSpan;
            Content = signatureItem.Content;
            Documentation = signatureItem.Documentation;
            Parameters = new ReadOnlyCollection<IParameter>(parameters);
            CurrentParameter = selectedParameter >= 0 && selectedParameter < parameters.Length
                ? parameters[selectedParameter]
                : null;
        }

        private Parameter CreateParameter(ParameterItem p)
        {
            return new Parameter(this, p.Name, p.Documentation, new Span(p.Span.Start, p.Span.Length));
        }

        public IParameter CurrentParameter
        {
            get { return _currentParameter; }
            internal set
            {
                if (_currentParameter != value)
                {
                    var prevCurrentParameter = _currentParameter;
                    _currentParameter = value;
                    RaiseCurrentParameterChanged(prevCurrentParameter, _currentParameter);
                }
            }
        }

        private void RaiseCurrentParameterChanged(IParameter prevCurrentParameter, IParameter newCurrentParameter)
        {
            var tempHandler = CurrentParameterChanged;
            if (tempHandler != null)
                tempHandler(this, new CurrentParameterChangedEventArgs(prevCurrentParameter, newCurrentParameter));
        }

        public ITrackingSpan ApplicableToSpan { get; }
        public string Content { get; }
        public string PrettyPrintedContent { get; }
        public string Documentation { get; }
        public ReadOnlyCollection<IParameter> Parameters { get; internal set; }
        public event EventHandler<CurrentParameterChangedEventArgs> CurrentParameterChanged;
    }
}