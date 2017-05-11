using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;

namespace ShaderTools.VisualStudio.LanguageServices.ErrorList
{
    [Export(typeof(ErrorsTableDataSource))]
    internal sealed class ErrorsTableDataSource : ITableDataSource
    {
        private readonly VisualStudioWorkspace _workspace;

        [ImportingConstructor]
        public ErrorsTableDataSource(
            ITableManagerProvider tableManagerProvider,
            VisualStudioWorkspace workspace)
        {
            _workspace = workspace;

            var manager = tableManagerProvider.GetTableManager(StandardTables.ErrorsTable);
            manager.AddSource(
                this,
                StandardTableColumnDefinitions.Column,
                StandardTableColumnDefinitions.Line,
                StandardTableColumnDefinitions.DocumentName,
                StandardTableColumnDefinitions.ErrorCode,
                StandardTableColumnDefinitions.ErrorSeverity,
                StandardTableColumnDefinitions.ErrorSource,
                StandardTableColumnDefinitions.Text);
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            return new ErrorsSinkManager(sink, _workspace);
        }

        public string SourceTypeIdentifier => StandardTableDataSources.ErrorTableDataSource;

        public string Identifier => "ShaderToolsErrorsTableDataSource";

        public string DisplayName => "Shader Tools";
    }
}
