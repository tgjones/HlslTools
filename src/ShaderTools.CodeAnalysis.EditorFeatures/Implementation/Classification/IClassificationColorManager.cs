using System.Windows.Media;

namespace ShaderTools.CodeAnalysis.Editor.Implementation.Classification
{
    internal interface IClassificationColorManager
    {
        Color GetDefaultColor(string category);

        void UpdateColors();
    }
}