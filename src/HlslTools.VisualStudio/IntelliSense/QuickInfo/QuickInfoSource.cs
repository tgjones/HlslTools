using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using HlslTools.Symbols.Markup;
using HlslTools.VisualStudio.Glyphs;
using HlslTools.VisualStudio.Tagging.Classification;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Formatting;
using Span = Microsoft.VisualStudio.Text.Span;

namespace HlslTools.VisualStudio.IntelliSense.QuickInfo
{
    internal sealed class QuickInfoSource : IQuickInfoSource
    {
        private readonly IClassificationFormatMap _classificationFormatMap;
        private readonly IClassificationFormatMap _tooltipClassificationFormatMap;
        private readonly HlslClassificationService _classificationService;
        private readonly DispatcherGlyphService _dispatcherGlyphService;

        public QuickInfoSource(
            IClassificationFormatMapService classificationFormatMapService,
            HlslClassificationService classificationService,
            DispatcherGlyphService dispatcherGlyphService)
        {
            _classificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("text");
            _tooltipClassificationFormatMap = classificationFormatMapService.GetClassificationFormatMap("tooltip");
            _classificationService = classificationService;
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
            var span = new Span(textSpan.Start, textSpan.Length);
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
            SetTextProperties(textBlock, _tooltipClassificationFormatMap.DefaultTextProperties, true);
            textBlock.Inlines.AddRange(markup.Tokens.Select(GetInline));
            return textBlock;
        }

        private Inline GetInline(SymbolMarkupToken markupToken)
        {
            switch (markupToken.Kind)
            {
                case SymbolMarkupKind.Keyword:
                    return GetClassifiedText(markupToken.Text, _classificationService.Keyword);
                case SymbolMarkupKind.Punctuation:
                    return GetClassifiedText(markupToken.Text, _classificationService.Punctuation);
                case SymbolMarkupKind.Whitespace:
                    return GetClassifiedText(markupToken.Text, _classificationService.WhiteSpace);
                case SymbolMarkupKind.LocalVariableName:
                    return GetClassifiedText(markupToken.Text, _classificationService.LocalVariableIdentifier);
                case SymbolMarkupKind.GlobalVariableName:
                    return GetClassifiedText(markupToken.Text, _classificationService.GlobalVariableIdentifier);
                case SymbolMarkupKind.ParameterName:
                    return GetClassifiedText(markupToken.Text, _classificationService.ParameterIdentifier);
                case SymbolMarkupKind.FunctionName:
                    return GetClassifiedText(markupToken.Text, _classificationService.FunctionIdentifier);
                case SymbolMarkupKind.MethodName:
                    return GetClassifiedText(markupToken.Text, _classificationService.FunctionIdentifier);
                case SymbolMarkupKind.FieldName:
                    return GetClassifiedText(markupToken.Text, _classificationService.FieldIdentifier);
                case SymbolMarkupKind.TypeName:
                    return GetClassifiedText(markupToken.Text, _classificationService.ClassIdentifier);
                case SymbolMarkupKind.NamespaceName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Identifier);
                case SymbolMarkupKind.SemanticName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Semantic);
                case SymbolMarkupKind.TechniqueName:
                    return GetClassifiedText(markupToken.Text, _classificationService.Identifier);
                case SymbolMarkupKind.PlainText:
                    return GetClassifiedText(markupToken.Text, _classificationService.Other);
                default:
                    throw new ArgumentOutOfRangeException(nameof(markupToken.Kind));
            }
        }

        private Inline GetClassifiedText(string text, IClassificationType classificationType)
        {
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