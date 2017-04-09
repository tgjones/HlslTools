using System.ComponentModel.Composition;
using ShaderTools.EditorServices.Workspace.Hlsl;

namespace ShaderTools.Editor.VisualStudio.Hlsl
{
    //[Export(typeof(HlslWorkspaceManager))]
    internal sealed class HlslWorkspaceManager
    {
        public HlslWorkspace Workspace { get; }

        public HlslWorkspaceManager()
        {
            Workspace = new HlslWorkspace();
        }
    }
}
