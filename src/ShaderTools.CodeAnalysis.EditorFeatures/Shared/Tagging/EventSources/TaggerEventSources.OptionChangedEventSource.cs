// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using Microsoft.VisualStudio.Text;
using ShaderTools.CodeAnalysis.Editor.Tagging;
using ShaderTools.CodeAnalysis.Options;
using ShaderTools.CodeAnalysis.Text;

namespace ShaderTools.CodeAnalysis.Editor.Shared.Tagging
{
    internal partial class TaggerEventSources
    {
        private class OptionChangedEventSource : AbstractWorkspaceTrackingTaggerEventSource
        {
            private readonly IOption _option;
            private IOptionService _optionService;
            private IOptionsService _optionsService;

            public OptionChangedEventSource(ITextBuffer subjectBuffer, IOption option, TaggerDelay delay) : base(subjectBuffer, delay)
            {
                _option = option;
            }

            protected override void ConnectToWorkspace(Workspace workspace)
            {
                _optionService = workspace.Services.GetService<IOptionService>();
                if (_optionService != null)
                {
                    _optionService.OptionChanged += OnOptionChanged;
                }

                // TODO: Remove this.
                _optionsService = base.SubjectBuffer.AsTextContainer().GetOpenDocumentInCurrentContext().LanguageServices.GetRequiredService<IOptionsService>();
                _optionsService.OptionsChanged += OnOptionsChanged;
            }

            protected override void DisconnectFromWorkspace(Workspace workspace)
            {
                if (_optionService != null)
                {
                    _optionService.OptionChanged -= OnOptionChanged;
                    _optionService = null;
                }

                if (_optionsService != null)
                {
                    _optionsService.OptionsChanged -= OnOptionsChanged;
                    _optionsService = null;
                }
            }

            private void OnOptionChanged(object sender, OptionChangedEventArgs e)
            {
                if (e.Option == _option)
                {
                    this.RaiseChanged();
                }
            }

            private void OnOptionsChanged(object sender, EventArgs e)
            {
                RaiseChanged();
            }
        }
    }
}
