// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Syntax;

namespace ShaderTools.CodeAnalysis.Formatting.Rules
{
    /// <summary>
    /// Provide a custom formatting operation provider that can intercept/filter/replace default formatting operations.
    /// </summary>
    /// <remarks>All methods defined in this interface can be called concurrently. Must be thread-safe.</remarks>
    internal interface IFormattingRule
    {
        ///// <summary>
        ///// Returns SuppressWrappingIfOnSingleLineOperations under a node either by itself or by
        ///// filtering/replacing operations returned by NextOperation
        ///// </summary>
        //void AddSuppressOperations(List<SuppressOperation> list, SyntaxNodeBase node, ISyntaxToken lastToken, OptionSet optionSet, NextAction<SuppressOperation> nextOperation);

        ///// <summary>
        ///// returns AnchorIndentationOperations under a node either by itself or by filtering/replacing operations returned by NextOperation
        ///// </summary>
        //void AddAnchorIndentationOperations(List<AnchorIndentationOperation> list, SyntaxNodeBase node, OptionSet optionSet, NextAction<AnchorIndentationOperation> nextOperation);

        ///// <summary>
        ///// returns IndentBlockOperations under a node either by itself or by filtering/replacing operations returned by NextOperation
        ///// </summary>
        //void AddIndentBlockOperations(List<IndentBlockOperation> list, SyntaxNodeBase node, OptionSet optionSet, NextAction<IndentBlockOperation> nextOperation);

        ///// <summary>
        ///// returns AlignTokensOperations under a node either by itself or by filtering/replacing operations returned by NextOperation
        ///// </summary>
        //void AddAlignTokensOperations(List<AlignTokensOperation> list, SyntaxNodeBase node, OptionSet optionSet, NextAction<AlignTokensOperation> nextOperation);

        ///// <summary>
        ///// returns AdjustNewLinesOperation between two tokens either by itself or by filtering/replacing a operation returned by NextOperation
        ///// </summary>
        //AdjustNewLinesOperation GetAdjustNewLinesOperation(ISyntaxToken previousToken, ISyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustNewLinesOperation> nextOperation);

        ///// <summary>
        ///// returns AdjustSpacesOperation between two tokens either by itself or by filtering/replacing a operation returned by NextOperation
        ///// </summary>
        //AdjustSpacesOperation GetAdjustSpacesOperation(ISyntaxToken previousToken, ISyntaxToken currentToken, OptionSet optionSet, NextOperation<AdjustSpacesOperation> nextOperation);
    }
}
