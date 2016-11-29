using Microsoft.VisualStudio.Shell;

namespace ShaderTools.VisualStudio.Hlsl.Editing.BraceCompletion
{
    internal sealed class ProvideBraceCompletionAttribute : RegistrationAttribute
    {
        private readonly string _languageName;

        public ProvideBraceCompletionAttribute(string languageName)
        {
            _languageName = languageName;
        }

        public override void Register(RegistrationContext context)
        {
            using (var serviceKey = context.CreateKey(LanguageServicesKeyName))
                serviceKey.SetValue("ShowBraceCompletion", 1);
        }

        public override void Unregister(RegistrationContext context)
        {
        }

        private string LanguageServicesKeyName => $"Languages\\Language Services\\{_languageName}";
    }
}