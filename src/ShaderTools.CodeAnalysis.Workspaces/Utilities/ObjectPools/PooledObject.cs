// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using ShaderTools.Utilities.PooledObjects;

namespace ShaderTools.Utilities
{
    /// <summary>
    /// this is RAII object to automatically release pooled object when its owning pool
    /// </summary>
    internal struct PooledObject<T> : IDisposable where T : class
    {
        private readonly Action<ObjectPool<T>, T> _releaser;
        private readonly ObjectPool<T> _pool;
        private T _pooledObject;

        public PooledObject(ObjectPool<T> pool, Func<ObjectPool<T>, T> allocator, Action<ObjectPool<T>, T> releaser) : this()
        {
            _pool = pool;
            _pooledObject = allocator(pool);
            _releaser = releaser;
        }

        public T Object => _pooledObject;

        public void Dispose()
        {
            if (_pooledObject != null)
            {
                _releaser(_pool, _pooledObject);
                _pooledObject = null;
            }
        }

        #region factory

        public static PooledObject<List<TItem>> Create<TItem>(ObjectPool<List<TItem>> pool)
        {
            return new PooledObject<List<TItem>>(pool, Allocator, Releaser);
        }
        #endregion

        #region allocators and releasers
        private static List<TItem> Allocator<TItem>(ObjectPool<List<TItem>> pool)
        {
            return pool.AllocateAndClear();
        }

        private static void Releaser<TItem>(ObjectPool<List<TItem>> pool, List<TItem> obj)
        {
            pool.ClearAndFree(obj);
        }
        #endregion
    }
}
