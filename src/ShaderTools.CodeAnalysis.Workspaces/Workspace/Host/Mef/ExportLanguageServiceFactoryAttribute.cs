using System;
using System.Composition;

namespace ShaderTools.CodeAnalysis.Host.Mef
{
    /// <summary>
    /// Use this attribute to declare a <see cref="ILanguageServiceFactory"/> implementation for inclusion in a MEF-based workspace.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportLanguageServiceFactoryAttribute : ExportAttribute
    {
        /// <summary>
        /// The assembly qualified name of the service's type.
        /// </summary>
        public string ServiceType { get; }

        /// <summary>
        /// The language that the service is target for; LanguageNames.CSharp, etc.
        /// </summary>
        public string Language { get; }

        /// <summary>
        /// Declares a <see cref="ILanguageServiceFactory"/> implementation for inclusion in a MEF-based workspace.
        /// </summary>
        /// <param name="type">The type that will be used to retrieve the service from a <see cref="HostLanguageServices"/>.</param>
        /// <param name="language">The language that the service is target for; LanguageNames.CSharp, etc.</param>
        public ExportLanguageServiceFactoryAttribute(Type type, string language)
            : base(typeof(ILanguageServiceFactory))
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            this.ServiceType = type.AssemblyQualifiedName;
            this.Language = language ?? throw new ArgumentNullException(nameof(language));
        }
    }
}
