using System;

namespace ShaderTools.VisualStudio.Core.Options
{
    internal interface IOptionsService
    {
        event EventHandler OptionsChanged;

        void RaiseOptionsChanged();

        bool EnableErrorReporting { get; }
        bool EnableSquiggles { get; }
    }
}
