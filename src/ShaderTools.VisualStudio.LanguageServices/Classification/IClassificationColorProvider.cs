using System.Collections.Immutable;
using System.Windows.Media;

namespace ShaderTools.VisualStudio.LanguageServices.Classification
{
    internal interface IClassificationColorProvider
    {
        ImmutableDictionary<string, Color> LightAndBlueColors { get; }
        ImmutableDictionary<string, Color> DarkColors { get; }
    }
}
