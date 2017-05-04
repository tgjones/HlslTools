// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Text;

namespace ShaderTools.CodeAnalysis.Text
{
    internal class StringTextWriter : SourceTextWriter
    {
        private readonly StringBuilder _builder;

        public StringTextWriter(int capacity)
        {
            _builder = new StringBuilder(capacity);
        }

        public override SourceText ToSourceText()
        {
            return new StringText(_builder.ToString(), null);
        }

        public override void Write(char value)
        {
            _builder.Append(value);
        }

        public override void Write(string value)
        {
            _builder.Append(value);
        }

        public override void Write(char[] buffer, int index, int count)
        {
            _builder.Append(buffer, index, count);
        }
    }
}