using System;

namespace HlslTools.Binding
{
    [Flags]
    internal enum SymbolFilter
    {
        Locals = 1,
        Members = 2,
        Types = 4,
        LocalTypes = 8,
        InstanceMembers = 32,
        StaticMembers = 64,
        ExcludeParent = 1024,
        AllTypes = Locals | Members | Types | LocalTypes,
        AnyMember = InstanceMembers | StaticMembers,
        All = AllTypes | AnyMember,
    }
}