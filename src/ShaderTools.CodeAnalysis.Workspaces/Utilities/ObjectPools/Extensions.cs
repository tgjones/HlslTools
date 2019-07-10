// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Text;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.Utilities
{
    internal static class SharedPoolExtensions
    {
        private const int Threshold = 512;

        public static PooledObject<List<TItem>> GetPooledObject<TItem>(this ObjectPool<List<TItem>> pool)
        {
            return PooledObject<List<TItem>>.Create(pool);
        }

        public static List<T> AllocateAndClear<T>(this ObjectPool<List<T>> pool)
        {
            var list = pool.Allocate();
            list.Clear();

            return list;
        }

        public static void ClearAndFree<T>(this ObjectPool<Stack<T>> pool, Stack<T> set)
        {
            if (set == null)
            {
                return;
            }

            var count = set.Count;
            set.Clear();

            if (count > Threshold)
            {
                set.TrimExcess();
            }

            pool.Free(set);
        }

        public static void ClearAndFree<T>(this ObjectPool<List<T>> pool, List<T> list)
        {
            if (list == null)
            {
                return;
            }

            list.Clear();

            if (list.Capacity > Threshold)
            {
                list.Capacity = Threshold;
            }

            pool.Free(list);
        }
    }
}
