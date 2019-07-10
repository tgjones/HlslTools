// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;

namespace ShaderTools.CodeAnalysis.Tags
{
    public static class WellKnownTags
    {
        // accessibility
        public const string Public = nameof(Public);
        public const string Protected = nameof(Protected);
        public const string Private = nameof(Private);
        public const string Internal = nameof(Internal);

        // project elements
        public const string File = nameof(File);
        public const string Project = nameof(Project);
        public const string Folder = nameof(Folder);
        public const string Assembly = nameof(Assembly);

        // language elements
        public const string Class = nameof(Class);
        public const string Constant = nameof(Constant);
        public const string Delegate = nameof(Delegate);
        public const string Enum = nameof(Enum);
        public const string EnumMember = nameof(EnumMember);
        public const string Event = nameof(Event);
        public const string ExtensionMethod = nameof(ExtensionMethod);
        public const string Field = nameof(Field);
        public const string Interface = nameof(Interface);
        public const string Intrinsic = nameof(Intrinsic);
        public const string Keyword = nameof(Keyword);
        public const string Label = nameof(Label);
        public const string Local = nameof(Local);
        public const string Namespace = nameof(Namespace);
        public const string Method = nameof(Method);
        public const string Module = nameof(Module);
        public const string Operator = nameof(Operator);
        public const string Parameter = nameof(Parameter);
        public const string Property = nameof(Property);
        public const string RangeVariable = nameof(RangeVariable);
        public const string Reference = nameof(Reference);
        public const string Structure = nameof(Structure);
        public const string TypeParameter = nameof(TypeParameter);

        // other
        public const string Snippet = nameof(Snippet);
        public const string Error = nameof(Error);
        public const string Warning = nameof(Warning);

        internal const string StatusInformation = nameof(StatusInformation);

        internal const string AddReference = nameof(AddReference);
        internal const string NuGet = nameof(NuGet);
    }
}