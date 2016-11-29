using System;

namespace ShaderTools.Hlsl.Syntax
{
    [Flags]
    public enum StorageFlags
    {
        None = 0,

        // Type modifiers
        Const = 0x01,
        RowMajor = 0x02,
        ColumnMajor = 0x04,

        // Storage classes.
        Extern = 0x08,
        Precise = 0x10,
        Shared = 0x20,
        Groupshared = 0x30,
        Static = 0x40,
        Uniform = 0x50,
        Volatile = 0x60,

        // Interpolation modifiers.
        Linear = 0x10000,
        Centroid = 0x20000,
        NoInterpolation = 0x40000,
        NoPerspective = 0x80000,
        Sample = 0x100000,

        // Parameter modifiers.
        In,
        Out,
        Inout,

        // Geometry shader primitive types.
        Point,
        Line,
        Triangle
    }
}