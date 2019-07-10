// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using ShaderTools.Utilities.Collections;

namespace ShaderTools.Utilities.Threading
{
    internal static class SpecializedTasks
    {
        public static readonly Task<bool> True = Task.FromResult<bool>(true);
        public static readonly Task<bool> False = Task.FromResult<bool>(false);
        public static readonly Task EmptyTask = Task.CompletedTask;

        public static Task<T> Default<T>()
        {
            return Empty<T>.Default;
        }

        private static class Empty<T>
        {
            public static readonly Task<T> Default = Task.FromResult<T>(default(T));
            public static readonly Task<IEnumerable<T>> EmptyEnumerable = Task.FromResult<IEnumerable<T>>(SpecializedCollections.EmptyEnumerable<T>());
            public static readonly Task<ImmutableArray<T>> EmptyImmutableArray = Task.FromResult(ImmutableArray<T>.Empty);
        }
    }
}
