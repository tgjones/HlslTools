using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using ShaderTools.EditorServices.Workspace;
using ShaderTools.EditorServices.Workspace.Host.Mef;

namespace ShaderTools.Editor.VisualStudio.Core
{
    [Export(typeof(VisualStudioWorkspace))]
    internal sealed class VisualStudioWorkspace : Workspace
    {
        [ImportingConstructor]
        public VisualStudioWorkspace(ExportProvider exportProvider)
            : base(MefV1HostServices.Create(exportProvider))
        {
        }
    }
}
