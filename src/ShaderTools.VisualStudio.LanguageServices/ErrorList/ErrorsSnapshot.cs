using System;
using System.Collections.Immutable;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using ShaderTools.CodeAnalysis.Diagnostics;

namespace ShaderTools.VisualStudio.LanguageServices.ErrorList
{
    internal sealed class ErrorsSnapshot : WpfTableEntriesSnapshotBase
    {
        private readonly ImmutableArray<MappedDiagnostic> _diagnostics;

        public override int Count => _diagnostics.Length;

        public override int VersionNumber { get; }

        public ErrorsSnapshot(
            ImmutableArray<MappedDiagnostic> diagnostics, 
            int versionNumber)
        {
            _diagnostics = diagnostics;
            VersionNumber = versionNumber;
        }

        public override int IndexOf(int currentIndex, ITableEntriesSnapshot newSnapshot)
        {
            // TODO
            return base.IndexOf(currentIndex, newSnapshot);
        }

        public override bool TryGetValue(int index, string keyName, out object content)
        {
            if (index < 0 || index >= _diagnostics.Length)
            {
                content = null;
                return false;
            }

            var diagnostic = _diagnostics[index];
            var diagnosticSpan = diagnostic.FileSpan.Span;
            var diagnosticFile = diagnostic.FileSpan.File;

            switch (keyName)
            {
                case StandardTableKeyNames.DocumentName:
                    content = diagnosticFile.FilePath;
                    return true;

                case StandardTableKeyNames.Line:
                    content = diagnosticFile.Text.Lines.GetLinePosition(diagnosticSpan.Start).Line;
                    return true;

                case StandardTableKeyNames.Column:
                    content = diagnosticFile.Text.Lines.GetLinePosition(diagnosticSpan.Start).Character;
                    return true;

                case StandardTableKeyNames.Text:
                    content = diagnostic.Diagnostic.Message;
                    return true;

                case StandardTableKeyNames.ErrorCode:
                    content = diagnostic.Diagnostic.Descriptor.Id;
                    return true;

                case StandardTableKeyNames.ErrorSeverity:
                    content = GetErrorSeverity(diagnostic.Diagnostic.Severity);
                    return true;

                case StandardTableKeyNames.ErrorSource:
                    content = ErrorSource.Other;
                    return true;

                // TODO: ErrorRank
                // TODO ErrorSource

                default:
                    content = null;
                    return false;
            }
        }

        private static __VSERRORCATEGORY GetErrorSeverity(DiagnosticSeverity severity)
        {
            switch (severity)
            {
                case DiagnosticSeverity.Warning:
                    return __VSERRORCATEGORY.EC_WARNING;

                case DiagnosticSeverity.Error:
                    return __VSERRORCATEGORY.EC_ERROR;

                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, null);
            }
        }
    }
}