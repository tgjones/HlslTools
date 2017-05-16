// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using ShaderTools.VisualStudio.LanguageServices.Options.UI;

namespace ShaderTools.VisualStudio.LanguageServices.Hlsl.Options.Formatting
{
    internal class FormattingSpacingPage : AbstractOptionPage
    {
        public FormattingSpacingPage()
        {
        }

        protected override AbstractOptionPageControl CreateOptionPage(IServiceProvider serviceProvider)
        {
            return new OptionPreviewControl(serviceProvider, (o, s) => new SpacingViewModel(o, s));
        }
    }
}
