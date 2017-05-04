// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace ShaderTools.CodeAnalysis.Text
{
    internal abstract class SourceTextWriter : TextWriter
    {
        public override Encoding Encoding => null;

        public abstract SourceText ToSourceText();

        public static SourceTextWriter Create(int length)
        {
            //if (length < SourceText.LargeObjectHeapLimitInChars)
            //{
            return new StringTextWriter(length);
            //            }
            //            else
            //            {
            //                return new LargeTextWriter(length);
            //            }
            //        }
        }
    }
}