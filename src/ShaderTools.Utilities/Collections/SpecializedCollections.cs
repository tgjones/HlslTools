// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ShaderTools.Utilities.Collections
{
    internal static partial class SpecializedCollections
    {
        public static IEnumerable<T> EmptyEnumerable<T>()
        {
            return Empty.List<T>.Instance;
        }

        public static IList<T> EmptyList<T>()
        {
            return Empty.List<T>.Instance;
        }

        public static IEnumerable<T> SingletonEnumerable<T>(T value)
        {
            return new Singleton.List<T>(value);
        }

        public static ICollection<T> SingletonCollection<T>(T value)
        {
            return new Singleton.List<T>(value);
        }

        public static IList<T> SingletonList<T>(T value)
        {
            return new Singleton.List<T>(value);
        }
    }
}
