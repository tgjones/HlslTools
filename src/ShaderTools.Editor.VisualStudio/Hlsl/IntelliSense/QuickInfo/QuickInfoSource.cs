using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using ShaderTools.CodeAnalysis.Classification;
using ShaderTools.CodeAnalysis.Editor.Shared.Utilities;
using ShaderTools.CodeAnalysis.Hlsl.Classification;
using ShaderTools.CodeAnalysis.Symbols.Markup;
using ShaderTools.Editor.VisualStudio.Core.Glyphs;
using Span = Microsoft.VisualStudio.Text.Span;

namespace ShaderTools.Editor.VisualStudio.Hlsl.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoSource : IQuickInfoSource
    {
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IClassificationFormatMap _tooltipClassificationFormatMap;
        private readonly ClassificationTypeMap _classificationTypeMap;
        private readonly DispatcherGlyphService _dispatcherGlyphService;

        public QuickInfoSource(
            IClassificationFormatMapService classificationFormatMapService,
            ClassificationTypeMap classificationTypeMap,
            DispatcherGlyphService dispatcherGlyphService)
        {
            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("text");
            _tooltipClassificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("tooltip");
            _classificationTypeMap = classificationTypeMap;
            _dispatcherGlyphService = dispatcherGlyphService;
        }

        public void Dispose()
        {
        }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> quickInfoContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            QuickInfoManager quickInfoManager;
            if (!session.Properties.TryGetProperty(typeof(QuickInfoManager), out quickInfoManager))
                return;

            var model = quickInfoManager.Model;
            var textSpan = model.Span;
            var span = new Span(textSpan.Span.Start, textSpan.Span.Length);
            var currentSnapshot = session.TextView.TextBuffer.CurrentSnapshot;
            var content = GetContent(model);
            if (content == null)
                return;

            applicableToSpan = currentSnapshot.CreateTrackingSpan(span, SpanTrackingMode.EdgeNegative);
            quickInfoContent.Add(content);
        }

        private FrameworkElement GetContent(QuickInfoModel model)
        {
            if (model.Markup.Tokens.Length == 0)
                return null;

            var glyph = GetGlyph(model.Glyph);
            var textBlock = GetTextBlock(model.Markup);
            var stackPanel = new StackPanel();
            stackPanel.Orientation = Orientation.Horizontal;
            stackPanel.Children.Add(glyph);
            stackPanel.Children.Add(textBlock);

            var container = new QuickInfoDisplayPanel();
            SetTextProperties(container, _tooltipClassificationFormatMap.DefaultTextProperties, true);
            container.Orientation = Orientation.Vertical;
            container.Children.Add(stackPanel);

            if (!string.IsNullOrEmpty(model.Documentation))
                container.Children.Add(new TextBlock
                {
                    Text = model.Documentation,
                    Margin = new Thickness(0, 3, 0, 0)
                });

            return container;
        }

        private sealed class QuickInfoDisplayPanel : StackPanel
        {
            
        }

        private Image GetGlyph(Glyph glyph)
        {
            var image = new Image
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0),
                Source = glyph.GetImageSource(_dispatcherGlyphService)
            };

            var binding = new System.Windows.Data.Binding("Background")
            {
                Converter = new BrushToColorConverter(),
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(QuickInfoDisplayPanel), 1)
            };

            image.SetBinding(ImageThemingUtilities.ImageBackgroundColorProperty, binding);
            return image;
        }

        private TextBlock GetTextBlock(SymbolMarkup markup)
        {
            var textBlock = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap
            };
            textBlock.Inlines.AddRange(markup.Tokens.Select(GetInline));
            return textBlock;
        }

        private Inline GetInline(SymbolMarkupToken markupToken)
        {
            switch (markupToken.Kind)
            {
                case SymbolMarkupKind.Keyword:
                    return GetClassifiedText(markupToken.Text, ClassificationTypeNames.Keyword);
                case SymbolMarkupKind.Punctuation:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.Punctuation);
                case SymbolMarkupKind.Whitespace:
                    return GetClassifiedText(markupToken.Text, ClassificationTypeNames.WhiteSpace);
                case SymbolMarkupKind.LocalVariableName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.LocalVariableIdentifier);
                case SymbolMarkupKind.ConstantBufferVariableName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.ConstantBufferVariableIdentifier);
                case SymbolMarkupKind.GlobalVariableName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.GlobalVariableIdentifier);
                case SymbolMarkupKind.ParameterName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.ParameterIdentifier);
                case SymbolMarkupKind.FunctionName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.FunctionIdentifier);
                case SymbolMarkupKind.MethodName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.MethodIdentifier);
                case SymbolMarkupKind.FieldName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.FieldIdentifier);
                case SymbolMarkupKind.IntrinsicTypeName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.ClassIdentifier);
                case SymbolMarkupKind.ClassName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.ClassIdentifier);
                case SymbolMarkupKind.StructName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.StructIdentifier);
                case SymbolMarkupKind.ConstantBufferName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.ConstantBufferIdentifier);
                case SymbolMarkupKind.InterfaceName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.InterfaceIdentifier);
                case SymbolMarkupKind.NamespaceName:
                    return GetClassifiedText(markupToken.Text, ClassificationTypeNames.Identifier);
                case SymbolMarkupKind.SemanticName:
                    return GetClassifiedText(markupToken.Text, HlslClassificationTypeNames.Semantic);
                case SymbolMarkupKind.TechniqueName:
                    return GetClassifiedText(markupToken.Text, ClassificationTypeNames.Identifier);
                case SymbolMarkupKind.PlainText:
                    return GetClassifiedText(markupToken.Text, ClassificationTypeNames.Text);
                default:
                    throw new ArgumentOutOfRangeException(nameof(markupToken.Kind));
            }
        }

        private Inline GetClassifiedText(string text, string classificationTypeName)
        {
            var classificationType = _classificationTypeMap.GetClassificationType(classificationTypeName);

            var properties = _classificationFormatMap.GetTextProperties(classificationType);

            var run = new Run(text);
            SetTextProperties(run, properties, false);
            run.TextDecorations = properties.TextDecorations;
            return run;
        }

        private static void SetTextProperties(DependencyObject dependencyObject, TextFormattingRunProperties properties, bool setFontFamily)
        {
            if (setFontFamily)
            {
                dependencyObject.SetValue(TextElement.FontFamilyProperty, properties.Typeface.FontFamily);
                dependencyObject.SetValue(TextElement.FontSizeProperty, properties.FontRenderingEmSize);
            }
            dependencyObject.SetValue(TextElement.FontStyleProperty, properties.Italic ? FontStyles.Italic : FontStyles.Normal);
            dependencyObject.SetValue(TextElement.FontWeightProperty, properties.Bold ? FontWeights.Bold : FontWeights.Normal);
            dependencyObject.SetValue(TextElement.ForegroundProperty, properties.ForegroundBrush);
            dependencyObject.SetValue(TextElement.BackgroundProperty, properties.BackgroundBrush);
            dependencyObject.SetValue(TextElement.TextEffectsProperty, properties.TextEffects);
        }
    }
}