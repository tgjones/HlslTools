// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.CodeAnalysis.Collections
{
    internal static class ArrayBuilderExtensions
    {
        public static void AddIfNotNull<T>(this ArrayBuilder<T> builder, T value)
            where T : class
        {
            if (value != null)
            {
                builder.Add(value);
            }
        }
    }
}
