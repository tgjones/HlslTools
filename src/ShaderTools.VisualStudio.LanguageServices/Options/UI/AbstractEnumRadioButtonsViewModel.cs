// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;

namespace ShaderTools.VisualStudio.LanguageServices.Options.UI
{
    internal abstract class AbstractEnumRadioButtonsViewModel
    {
        public string Description { get; }

        public List<AbstractRadioButtonViewModel> Items { get; protected set; }

        protected AbstractEnumRadioButtonsViewModel(string description)
        {
            Description = description;
        }
    }
}
