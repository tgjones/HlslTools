// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.CodeAnalysis.Options;
using OptionSet = ShaderTools.CodeAnalysis.Options.OptionSet;

namespace ShaderTools.VisualStudio.LanguageServices.Options.UI
{
    internal class EnumRadioButtonsViewModel<TOption> : AbstractEnumRadioButtonsViewModel
        where TOption : Enum
    {
        public EnumRadioButtonsViewModel(string description, string preview, string group, Option<TOption> option, AbstractOptionPreviewViewModel info, OptionSet options)
            : base(description)
        {
            var items = new List<AbstractRadioButtonViewModel>();

            var enumType = typeof(TOption);
            foreach (var value in Enum.GetValues(enumType))
            {
                var name = Enum.GetName(enumType, value);

                var memberInfo = enumType.GetMember(name)[0];
                var descriptionAttributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                var valueDescription = descriptionAttributes.Length > 0
                    ? ((DescriptionAttribute)descriptionAttributes[0]).Description
                    : name;

                items.Add(new RadioButtonViewModel<TOption>(
                    valueDescription, 
                    preview, 
                    group, 
                    (TOption)value,
                    option, 
                    info,
                    options));
            }

            Items = items;
        }
    }
}
