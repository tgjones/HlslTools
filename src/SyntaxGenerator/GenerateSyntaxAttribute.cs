using System;
using System.Diagnostics;
using CodeGeneration.Roslyn;

namespace SyntaxGenerator
{
    [AttributeUsage(AttributeTargets.Assembly)]
    [CodeGenerationAttribute(typeof(SyntaxCodeGenerator))]
    [Conditional("CodeGeneration")]
    public class GenerateSyntaxAttribute : Attribute
    {
        /// <summary>
        /// Relative path from the project root to the syntax file.
        /// </summary>
        public string SyntaxFilePath { get; }

        public GenerateSyntaxAttribute(string syntaxFilePath)
        {
            SyntaxFilePath = syntaxFilePath;
        }
    }
}
