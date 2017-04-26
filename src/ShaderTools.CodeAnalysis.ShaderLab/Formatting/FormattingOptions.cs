using System;

namespace ShaderTools.CodeAnalysis.ShaderLab.Formatting
{
    public class FormattingOptions
    {
        public int? SpacesPerIndent { get; set; }
        public string NewLine { get; set; }

        public FormattingOptions()
        {
            SpacesPerIndent = 4;
            NewLine = Environment.NewLine;
        }
    }
}