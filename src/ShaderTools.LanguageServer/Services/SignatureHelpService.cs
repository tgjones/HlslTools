using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;
using ShaderTools.CodeAnalysis.Shared.Utilities;
using ShaderTools.CodeAnalysis.SignatureHelp;
using ModelSignatureHelp = OmniSharp.Extensions.LanguageServer.Protocol.Models.SignatureHelp;

namespace ShaderTools.LanguageServer.Services.SignatureHelp
{
    [ExportWorkspaceService(typeof(SignatureHelpService))]
    internal sealed class SignatureHelpService : IWorkspaceService
    {
        private readonly IList<Lazy<ISignatureHelpProvider, OrderableLanguageMetadata>> _signatureHelpProviders;

        [ImportingConstructor]
        public SignatureHelpService(
            [ImportMany] IEnumerable<Lazy<ISignatureHelpProvider, OrderableLanguageMetadata>> signatureHelpProviders)
        {
            _signatureHelpProviders = ExtensionOrderer.Order(signatureHelpProviders);
        }

        // TODO: Some of this is duplicated from Controller.Session_ComputeModel.cs
        private async Task<SignatureHelpItems> GetItemsAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var triggerInfo = new SignatureHelpTriggerInfo(SignatureHelpTriggerReason.InvokeSignatureHelpCommand);

            var providers = _signatureHelpProviders
                .Where(x => x.Metadata.Language == document.Language)
                .Select(x => x.Value);

            ISignatureHelpProvider bestProvider = null;
            SignatureHelpItems bestItems = null;

            foreach (var provider in providers)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentItems = await provider.GetItemsAsync(document, position, triggerInfo, cancellationToken);
                if (currentItems != null && currentItems.ApplicableSpan.IntersectsWith(position))
                {
                    // If another provider provides sig help items, then only take them if they
                    // start after the last batch of items.  i.e. we want the set of items that
                    // conceptually are closer to where the caret position is.  This way if you have:
                    //
                    //  Foo(new Bar($$
                    //
                    // Then invoking sig help will only show the items for "new Bar(" and not also
                    // the items for "Foo(..."
                    if (IsBetter(bestItems, currentItems.ApplicableSpan))
                    {
                        bestItems = currentItems;
                        bestProvider = provider;
                    }
                }
            }

            return bestItems;
        }

        private bool IsBetter(SignatureHelpItems bestItems, TextSpan? currentTextSpan)
        {
            // If we have no best text span, then this span is definitely better.
            if (bestItems == null)
            {
                return true;
            }

            // Otherwise we want the one that is conceptually the innermost signature.  So it's
            // only better if the distance from it to the caret position is less than the best
            // one so far.
            return currentTextSpan.Value.Start > bestItems.ApplicableSpan.Start;
        }

        public async Task<ModelSignatureHelp> GetResultAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var signatureHelpItems = await GetItemsAsync(document, position, cancellationToken);

            if (signatureHelpItems != null)
            {
                return new ModelSignatureHelp
                {
                    Signatures = signatureHelpItems.Items
                        .Select(x => ConvertSignature(x, cancellationToken))
                        .ToArray(),
                    ActiveParameter = signatureHelpItems.ArgumentIndex,
                    ActiveSignature = signatureHelpItems.SelectedItemIndex
                };
            }
            else
            {
                return new ModelSignatureHelp
                {
                    Signatures = new SignatureInformation[0]
                };
            }
        }

        private static SignatureInformation ConvertSignature(SignatureHelpItem signatureHelpItem, CancellationToken cancellationToken)
        {
            var fullLabel = new List<TaggedText>();

            fullLabel.AddRange(signatureHelpItem.PrefixDisplayParts);
            for (var i = 0; i < signatureHelpItem.Parameters.Length; i++)
            {
                fullLabel.AddRange(signatureHelpItem.Parameters[i].DisplayParts);
                if (i < signatureHelpItem.Parameters.Length - 1)
                {
                    fullLabel.AddRange(signatureHelpItem.SeparatorDisplayParts);
                }
            }
            fullLabel.AddRange(signatureHelpItem.SuffixDisplayParts);

            return new SignatureInformation
            {
                Label = fullLabel.GetFullText(),
                Documentation = signatureHelpItem.DocumentationFactory(cancellationToken).GetFullText(),
                Parameters = signatureHelpItem.Parameters.Select(x => ConvertParameter(x, cancellationToken)).ToArray()
            };
        }

        private static ParameterInformation ConvertParameter(SignatureHelpParameter parameter, CancellationToken cancellationToken)
        {
            return new ParameterInformation
            {
                Label = parameter.Name,
                Documentation = parameter.DocumentationFactory(cancellationToken).GetFullText()
            };
        }
    }
}
