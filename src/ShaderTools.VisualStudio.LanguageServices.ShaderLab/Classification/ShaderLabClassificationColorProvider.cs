using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Windows.Media;
using ShaderTools.CodeAnalysis.ShaderLab.Classification;
using ShaderTools.VisualStudio.LanguageServices.Classification;

namespace ShaderTools.VisualStudio.LanguageServices.ShaderLab.Classification
{
    [Export(typeof(IClassificationColorProvider))]
    internal sealed class ShaderLabClassificationColorProvider : IClassificationColorProvider
    {
        public ImmutableDictionary<string, Color> LightAndBlueColors
        {
            get
            {
                return new Dictionary<string, Color>
                {
                    { ShaderLabClassificationTypeNames.Punctuation, Colors.Black },
                    { ShaderLabClassificationTypeNames.ShaderProperty, Color.FromRgb(139, 0, 139) },
                    { ShaderLabClassificationTypeNames.Attribute, Color.FromRgb(0, 0, 139) }
                }.ToImmutableDictionary();
            }
        }

        public ImmutableDictionary<string, Color> DarkColors
        {
            get
            {
                return new Dictionary<string, Color>
                {
                    { ShaderLabClassificationTypeNames.Punctuation, Colors.White },
                    { ShaderLabClassificationTypeNames.ShaderProperty, Color.FromRgb(221, 160, 221) },
                    { ShaderLabClassificationTypeNames.Attribute, Color.FromRgb(173, 216, 230) }
                }.ToImmutableDictionary();
            }
        }
    }
}
