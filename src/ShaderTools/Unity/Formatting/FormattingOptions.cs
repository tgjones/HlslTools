using System;

namespace ShaderTools.Unity.Formatting
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