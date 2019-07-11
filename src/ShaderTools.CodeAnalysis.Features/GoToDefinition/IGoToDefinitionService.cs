using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using ShaderTools.CodeAnalysis.FindUsages;

namespace ShaderTools.CodeAnalysis.GoToDefinition
{
    internal interface IGoToDefinitionService : ILanguageService
    {
        /// <summary>
        /// Finds the definitions for the symbol at the specific position in the document.
        /// </summary>
        Task<ImmutableArray<DefinitionItem>> FindDefinitionsAsync(Document document, int position, CancellationToken cancellationToken);
    }
}
