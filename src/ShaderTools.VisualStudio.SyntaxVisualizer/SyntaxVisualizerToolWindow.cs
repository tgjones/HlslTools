using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.SyntaxVisualizer
{
    [Guid("86efe633-4513-42b9-b315-0b515a0d607b")]
    public class SyntaxVisualizerToolWindow : ToolWindowPane
    {
        public SyntaxVisualizerToolWindow()
            : base(null)
        {
            Caption = "Shader Syntax Visualizer";

            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            BitmapResourceID = 301;
            BitmapIndex = 1;

            Content = new SyntaxVisualizerToolWindowControl();
        }
    }
}
