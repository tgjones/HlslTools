using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace ShaderTools.VisualStudio.ShaderLab.Tagging.Classification
{
    internal sealed class SemanticClassificationMetadata
    {
        public const string PunctuationClassificationTypeName = "ShaderLab.Punctuation";

#pragma warning disable 649

        [Export]
        [Name(PunctuationClassificationTypeName)]
        [BaseDefinition(PredefinedClassificationTypeNames.FormalLanguage)]
        public ClassificationTypeDefinition PunctuationType;

#pragma warning restore 649

        [Export(typeof(EditorFormatDefinition))]
        [Name(PunctuationClassificationTypeName)]
        [ClassificationType(ClassificationTypeNames = PunctuationClassificationTypeName)]
        [UserVisible(true)]
        [Order(After = PredefinedClassificationTypeNames.String)]
        public sealed class PunctuationFormat : ClassificationFormatDefinition
        {
            [ImportingConstructor]
            public PunctuationFormat(ClassificationColorManager colorManager)
            {
                DisplayName = "ShaderLab Punctuation";
                ForegroundColor = colorManager.GetDefaultColor(PunctuationClassificationTypeName);
            }
        }
    }
}