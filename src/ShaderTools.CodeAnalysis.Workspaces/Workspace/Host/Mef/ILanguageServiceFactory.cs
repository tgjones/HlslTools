using ILanguageService = Microsoft.CodeAnalysis.Host.ILanguageService;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    /// <summary>
    /// A factory that creates instances of a specific <see cref="ILanguageService"/>.
    /// 
    /// Implement a <see cref="ILanguageServiceFactory"/> when you want to provide <see cref="ILanguageService"/> instances that use other services.
    /// </summary>
    public interface ILanguageServiceFactory
    {
        /// <summary>
        /// Creates a new <see cref="ILanguageService"/> instance.
        /// </summary>
        /// <param name="languageServices">The <see cref="HostLanguageServices"/> that can be used to access other services.</param>
        ILanguageService CreateLanguageService(HostLanguageServices languageServices);
    }
}
