namespace ShaderTools.CodeAnalysis.QuickInfo
{
    internal interface IQuickInfoProviderCoordinatorFactory
    {
        IQuickInfoProviderCoordinator CreateCoordinator(Document document);
    }
}