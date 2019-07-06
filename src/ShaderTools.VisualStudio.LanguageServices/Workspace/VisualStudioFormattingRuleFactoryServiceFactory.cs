using System.Collections.Generic;
using Microsoft.CodeAnalysis.Text;
using ShaderTools.CodeAnalysis;
using ShaderTools.CodeAnalysis.Formatting.Rules;
using ShaderTools.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.Host.Mef;

namespace ShaderTools.VisualStudio.LanguageServices
{
    [ExportWorkspaceServiceFactory(typeof(IHostDependentFormattingRuleFactoryService))]
    internal sealed class VisualStudioFormattingRuleFactoryServiceFactory : IWorkspaceServiceFactory
    {
        public IWorkspaceService CreateService(HostWorkspaceServices workspaceServices)
        {
            return new Factory();
        }

        // TODO
        private sealed class Factory : IHostDependentFormattingRuleFactoryService
        {
            public IFormattingRule CreateRule(Document document, int position)
            {
                return null;
            }

            public IEnumerable<TextChange> FilterFormattedChanges(Document document, TextSpan span, IList<TextChange> changes)
            {
                return changes;
            }

            public bool ShouldNotFormatOrCommitOnPaste(Document document)
            {
                return false;
            }

            public bool ShouldUseBaseIndentation(Document document)
            {
                return false;
            }
        }
    }
}
