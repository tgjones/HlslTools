// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace ShaderTools.EditorServices.Utility
{
    internal static class TaskExtensions
    {
        // Only call this *extremely* special situations.  This will synchronously block a threadpool
        // thread.  In the future we are going ot be removing this and disallowing its use.
        public static T WaitAndGetResult_CanCallOnBackground<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            task.Wait(cancellationToken);
            return task.Result;
        }
    }
}
