// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
namespace ShaderTools.CodeAnalysis.Notification
{
    [Export(typeof(ISemanticChangeNotificationService)), Shared]
    internal class SemanticChangeNotificationService : ISemanticChangeNotificationService
    {
        public event EventHandler<Document> OpenedDocumentSemanticChanged;

        // TODO: Invoke this.
        private void RaiseOpenDocumentSemanticChangedEvent(Document document)
        {
            this.OpenedDocumentSemanticChanged?.Invoke(this, document);
        }
    }
}
