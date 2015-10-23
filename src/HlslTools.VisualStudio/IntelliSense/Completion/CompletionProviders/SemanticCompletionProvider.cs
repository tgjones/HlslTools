using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using HlslTools.Compilation;
using HlslTools.Syntax;
using HlslTools.VisualStudio.Glyphs;

namespace HlslTools.VisualStudio.IntelliSense.Completion.CompletionProviders
{
    [Export(typeof(ICompletionProvider))]
    internal sealed class SemanticCompletionProvider : CompletionProvider<SemanticSyntax>
    {
        private static readonly string[] StandardSemantics =
        {
            // D3D10
            "SV_ClipDistance",
            "SV_CullDistance",
            "SV_Coverage",
            "SV_Depth",
            "SV_DepthGreaterEqual",
            "SV_DepthLessEqual",
            "SV_DispatchThreadID",
            "SV_DomainLocation",
            "SV_GroupID",
            "SV_GroupIndex",
            "SV_GroupThreadID",
            "SV_GSInstanceID",
            "SV_InnerCoverage",
            "SV_InsideTessFactor",
            "SV_InstanceID",
            "SV_IsFrontFace",
            "SV_OutputControlPointID",
            "SV_Position",
            "SV_PrimitiveID",
            "SV_RenderTargetArrayIndex",
            "SV_SampleIndex",
            "SV_StencilRef",
            "SV_Target",
            "SV_TessFactor",
            "SV_VertexID",
            "SV_ViewportArrayIndex",

            // Vertex shader

            // Input
            "BINORMAL",
            "BLENDINDICES",
            "BLENDWEIGHT",
            "COLOR",
            "NORMAL",
            "POSITION",
            "POSITIONT",
            "PSIZE",
            "TANGENT",
            "TEXCOORD",

            // Output
            "FOG",
            "TESSFACTOR",

            // Pixel shader

            // Input
            "VFACE",
            "VPOS",

            // Output
            "DEPTH"
        };

        protected override IEnumerable<CompletionItem> GetItems(SemanticModel semanticModel, SourceLocation position, SemanticSyntax node)
        {
            if (node.ColonToken.IsMissing || position < node.ColonToken.SourceRange.End)
                return Enumerable.Empty<CompletionItem>();

            return StandardSemantics
                .Select(x => new CompletionItem(x, x, null, Glyph.Semantic));

        }
    }
}