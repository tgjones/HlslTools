using System;
using HlslTools.Symbols;
using HlslTools.Syntax;

namespace HlslTools.Binding
{
    public sealed class Conversion
    {
        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            Exists = exists;
            IsIdentity = isIdentity;
            IsImplicit = isImplicit;
        }

        private static readonly Conversion None = new Conversion(false, false, false);
        private static readonly Conversion Identity = new Conversion(true, true, true);
        private static readonly Conversion Implicit = new Conversion(true, false, true);
        private static readonly Conversion Explicit = new Conversion(true, false, false);
        private static readonly Conversion UpCast = new Conversion(true, false, true);
        private static readonly Conversion DownCast = new Conversion(true, false, false);

        public bool Exists { get; }

        public bool IsIdentity { get; }

        public bool IsImplicit { get; }

        public bool IsExplicit => Exists && !IsImplicit;

        internal static Conversion Classify(TypeSymbol sourceType, TypeSymbol targetType)
        {
            if (sourceType.Equals(targetType))
                return Identity;

            // Can convert from any scalar to any scalar.
            if (sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicScalarType)
                return Implicit;

            // Can convert from any scalar to any vector.
            if (sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicVectorType)
                return Implicit;

            // Can convert from any scalar to any matrix.
            if (sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicVectorType)
                return Implicit;

            // Can convert from any vector to any scalar.
            if (sourceType.Kind == SymbolKind.IntrinsicVectorType && targetType.Kind == SymbolKind.IntrinsicScalarType)
                return Implicit;

            // Can convert from any matrix to any scalar.
            if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicScalarType)
                return Implicit;

            // Can convert from any vector to same or smaller vector.
            if (sourceType.Kind == SymbolKind.IntrinsicVectorType && targetType.Kind == SymbolKind.IntrinsicVectorType)
            {
                var source = (IntrinsicVectorTypeSymbol) sourceType;
                var target = (IntrinsicVectorTypeSymbol) targetType;
                if (target.NumComponents <= source.NumComponents)
                    return Implicit;
            }

            // Can convert from any matrix to same or smaller matrix (of any type).
            if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicMatrixType)
            {
                var source = (IntrinsicMatrixTypeSymbol) sourceType;
                var target = (IntrinsicMatrixTypeSymbol) targetType;
                if (target.Rows <= source.Rows && target.Cols <= source.Cols)
                    return Implicit;
            }

            // Can convert from single column-or-row matrix to same or smaller vector (of different type).
            if (sourceType.Kind == SymbolKind.IntrinsicMatrixType && targetType.Kind == SymbolKind.IntrinsicVectorType)
            {
                var source = (IntrinsicMatrixTypeSymbol) sourceType;
                if (source.Rows == 1 || source.Cols == 1)
                {
                    var target = (IntrinsicVectorTypeSymbol) targetType;
                    var activeMatrixDimension = source.Rows == 1 ? source.Cols : source.Rows;
                    if (target.NumComponents <= activeMatrixDimension)
                        return Implicit;
                }
            }

            // Can convert from any numeric type to a struct.
            if (sourceType.IsIntrinsicNumericType() && targetType.Kind == SymbolKind.Struct)
                return Explicit;

            // TODO: Convert between inherited and base class references.
            // TODO: Array to numeric type conversions.

            return None;
        }

        internal static int GetScore(TypeSymbol sourceType, TypeSymbol targetType, Conversion conversion)
        {
            if (conversion.IsIdentity)
                return 0;

            var score = 0;

            if (conversion.IsExplicit)
                score += 1;

            if (sourceType.Kind == SymbolKind.IntrinsicScalarType && targetType.Kind == SymbolKind.IntrinsicScalarType)
            {
                var source = (IntrinsicScalarTypeSymbol) sourceType;
                var target = (IntrinsicScalarTypeSymbol) targetType;
                if (IsWidening(source.ScalarType, target.ScalarType))
                    score += 1;
                else
                    score += 6;
            }
            else
            {
                // TODO: Cost of vector / matrix conversions.
                score += 2;
            }

            return score;
        }

        private static bool IsWidening(ScalarType source, ScalarType target)
        {
            switch (source)
            {
                case ScalarType.Bool:
                    return target >= ScalarType.Bool;
                case ScalarType.Int:
                    return target == ScalarType.Int || target >= ScalarType.Half;
                case ScalarType.Uint:
                    return target == ScalarType.Uint || target >= ScalarType.Half;
                case ScalarType.Half:
                    return target >= ScalarType.Half;
                case ScalarType.Float:
                    return target >= ScalarType.Float;
                case ScalarType.Double:
                    return target == ScalarType.Double;
                case ScalarType.Void:
                case ScalarType.String:
                    return false; // TODO: Shouldn't be here
                default:
                    throw new ArgumentOutOfRangeException(nameof(source), source, null);
            }
        }
    }
}