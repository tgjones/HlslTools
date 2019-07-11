// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

namespace ShaderTools.Utilities.PooledObjects
{
    [DebuggerDisplay("Count = {Count,nq}")]
    [DebuggerTypeProxy(typeof(ArrayBuilder<>.DebuggerProxy))]
    internal sealed partial class ArrayBuilder<T> : IReadOnlyCollection<T>, IReadOnlyList<T>
    {
        #region DebuggerProxy

        private sealed class DebuggerProxy
        {
            private readonly ArrayBuilder<T> _builder;

            public DebuggerProxy(ArrayBuilder<T> builder)
            {
                _builder = builder;
            }

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public T[] A
            {
                get
                {
                    var result = new T[_builder.Count];
                    for (int i = 0; i < result.Length; i++)
                    {
                        result[i] = _builder[i];
                    }

                    return result;
                }
            }
        }

        #endregion

        private readonly ImmutableArray<T>.Builder _builder;

        private readonly ObjectPool<ArrayBuilder<T>> _pool;

        public ArrayBuilder(int size)
        {
            _builder = ImmutableArray.CreateBuilder<T>(size);
        }

        public ArrayBuilder() :
            this(8)
        { }

        private ArrayBuilder(ObjectPool<ArrayBuilder<T>> pool) :
            this()
        {
            _pool = pool;
        }

        /// <summary>
        /// Realizes the array.
        /// </summary>
        public ImmutableArray<T> ToImmutable()
        {
            return _builder.ToImmutable();
        }

        public int Count
        {
            get
            {
                return _builder.Count;
            }
            set
            {
                _builder.Count = value;
            }
        }

        public T this[int index]
        {
            get
            {
                return _builder[index];
            }

            set
            {
                _builder[index] = value;
            }
        }

        public void Add(T item)
        {
            _builder.Add(item);
        }

        public void EnsureCapacity(int capacity)
        {
            if (_builder.Capacity < capacity)
            {
                _builder.Capacity = capacity;
            }
        }

        public void Clear()
        {
            _builder.Clear();
        }

        public void RemoveAt(int index)
        {
            _builder.RemoveAt(index);
        }

        public T First()
        {
            return _builder[0];
        }

        /// <summary>
        /// Realizes the array and disposes the builder in one operation.
        /// </summary>
        public ImmutableArray<T> ToImmutableAndFree()
        {
            var result = this.ToImmutable();
            this.Free();
            return result;
        }

        #region Poolable

        // To implement Poolable, you need two things:
        // 1) Expose Freeing primitive. 
        public void Free()
        {
            var pool = _pool;
            if (pool != null)
            {
                // According to the statistics of a C# compiler self-build, the most commonly used builder size is 0.  (808003 uses).
                // The distant second is the Count == 1 (455619), then 2 (106362) ...
                // After about 50 (just 67) we have a long tail of infrequently used builder sizes.
                // However we have builders with size up to 50K   (just one such thing)
                //
                // We do not want to retain (potentially indefinitely) very large builders 
                // while the chance that we will need their size is diminishingly small.
                // It makes sense to constrain the size to some "not too small" number. 
                // Overall perf does not seem to be very sensitive to this number, so I picked 128 as a limit.
                if (this.Count < 128)
                {
                    if (this.Count != 0)
                    {
                        this.Clear();
                    }

                    pool.Free(this);
                    return;
                }
                else
                {
                    pool.ForgetTrackedObject(this);
                }
            }
        }

        // 2) Expose the pool or the way to create a pool or the way to get an instance.
        //    for now we will expose both and figure which way works better
        private static readonly ObjectPool<ArrayBuilder<T>> s_poolInstance = CreatePool();
        public static ArrayBuilder<T> GetInstance()
        {
            var builder = s_poolInstance.Allocate();
            Debug.Assert(builder.Count == 0);
            return builder;
        }

        public static ArrayBuilder<T> GetInstance(int capacity)
        {
            var builder = GetInstance();
            builder.EnsureCapacity(capacity);
            return builder;
        }

        public static ObjectPool<ArrayBuilder<T>> CreatePool()
        {
            return CreatePool(128); // we rarely need more than 10
        }

        public static ObjectPool<ArrayBuilder<T>> CreatePool(int size)
        {
            ObjectPool<ArrayBuilder<T>> pool = null;
            pool = new ObjectPool<ArrayBuilder<T>>(() => new ArrayBuilder<T>(pool), size);
            return pool;
        }

        #endregion

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void AddRange(ArrayBuilder<T> items)
        {
            _builder.AddRange(items._builder);
        }

        public void AddRange(ImmutableArray<T> items)
        {
            _builder.AddRange(items);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _builder.AddRange(items);
        }
    }
}
