using System;
using HlslTools.Symbols;

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
            if (sourceType == targetType)
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

            // TODO: Convert between inherited and base class references.

            return None;
        }

        internal static int Compare(TypeSymbol xType, Conversion xConversion, TypeSymbol yType, Conversion yConversion)
        {
            if (xConversion.IsIdentity && !yConversion.IsIdentity ||
                xConversion.IsImplicit && yConversion.IsExplicit)
                return -1;

            if (!xConversion.IsIdentity && yConversion.IsIdentity ||
                xConversion.IsExplicit && yConversion.IsImplicit)
                return 1;

            var xTypeToYType = Classify(xType, yType);
            var yTypeToXType = Classify(yType, xType);

            if (xTypeToYType.IsImplicit && yTypeToXType.IsExplicit)
                return -1;

            if (xTypeToYType.IsExplicit && yTypeToXType.IsImplicit)
                return 1;

            //var xKnown = xType.GetKnownType();
            //var yKnown = yType.GetKnownType();

            //if (xKnown != null && yKnown != null)
            //{
            //    var x = xKnown.Value;
            //    var y = yKnown.Value;

            //    if (x.IsSignedNumericType() && y.IsUnsignedNumericType())
            //        return -1;

            //    if (x.IsUnsignedNumericType() && y.IsSignedNumericType())
            //        return 1;
            //}

            return 0;
        }
    }
}