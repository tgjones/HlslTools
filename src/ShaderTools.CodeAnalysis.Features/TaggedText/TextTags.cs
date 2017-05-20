// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

namespace ShaderTools.CodeAnalysis
{
    /// <summary>
    /// The set of well known text tags used for the <see cref="TaggedText.Tag"/> property.
    /// These tags influence the presentation of text.
    /// </summary>
    public static class TextTags
    {
        public const string Alias = nameof(Alias);
        public const string Class = nameof(Class);
        public const string ConstantBuffer = nameof(ConstantBuffer);
        public const string ConstantBufferField = nameof(ConstantBufferField);
        public const string ErrorType = nameof(ErrorType);
        public const string Field = nameof(Field);
        public const string Function = nameof(Function);
        public const string Global = nameof(Global);
        public const string Interface = nameof(Interface);
        public const string Keyword = nameof(Keyword);
        public const string LineBreak = nameof(LineBreak);
        public const string Local = nameof(Local);
        public const string Method = nameof(Method);
        public const string Namespace = nameof(Namespace);
        public const string NumericLiteral = nameof(NumericLiteral);
        public const string Operator = nameof(Operator);
        public const string PackOffset = nameof(PackOffset);
        public const string Parameter = nameof(Parameter);
        public const string Property = nameof(Property);
        public const string Punctuation = nameof(Punctuation);
        public const string RegisterLocation = nameof(RegisterLocation);
        public const string Semantic = nameof(Semantic);
        public const string Space = nameof(Space);
        public const string StringLiteral = nameof(StringLiteral);
        public const string Struct = nameof(Struct);
        public const string Technique = nameof(Technique);
        public const string Text = nameof(Text);

    }
}