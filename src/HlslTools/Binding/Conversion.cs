using System;
using HlslTools.Symbols;

namespace HlslTools.Binding
{
    public sealed class Conversion
    {
        private const bool N = false;
        private const bool Y = true;

        private static readonly bool[,] ImplicitNumericConversions =
        {
            /*                SByte     Byte     Short     UShort     Int     UInt     Long     ULong     Char     Float     Double*/
            /* SByte   */  {  N,        N,       Y,        N,         Y,      N,       Y,       N,        N,       Y,        Y},
            /* Byte    */  {  N,        N,       Y,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Short   */  {  N,        N,       N,        N,         Y,      N,       Y,       N,        N,       Y,        Y},
            /* UShort  */  {  N,        N,       N,        N,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Int     */  {  N,        N,       N,        N,         N,      N,       Y,       N,        N,       Y,        Y},
            /* UInt    */  {  N,        N,       N,        N,         N,      N,       Y,       Y,        N,       Y,        Y},
            /* Long    */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y},
            /* ULong   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       Y,        Y},
            /* Char    */  {  N,        N,       N,        Y,         Y,      Y,       Y,       Y,        N,       Y,        Y},
            /* Float   */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        Y},
            /* Double  */  {  N,        N,       N,        N,         N,      N,       N,       N,        N,       N,        N}
        };

        private readonly bool _exists;
        private readonly bool _isIdentity;
        private readonly bool _isImplicit;

        private Conversion(bool exists, bool isIdentity, bool isImplicit)
        {
            _exists = exists;
            _isIdentity = isIdentity;
            _isImplicit = isImplicit;
        }

        private static readonly Conversion None = new Conversion(false, false, false);
        private static readonly Conversion Identity = new Conversion(true, true, true);
        private static readonly Conversion Implicit = new Conversion(true, false, true);
        private static readonly Conversion Explicit = new Conversion(true, false, false);
        private static readonly Conversion UpCast = new Conversion(true, false, true);
        private static readonly Conversion DownCast = new Conversion(true, false, false);

        public bool Exists
        {
            get { return _exists; }
        }

        public bool IsIdentity
        {
            get { return _isIdentity; }
        }

        public bool IsImplicit
        {
            get { return _isImplicit; }
        }

        public bool IsExplicit
        {
            get { return Exists && !IsImplicit; }
        }

        internal static Conversion Classify(TypeSymbol sourceType, TypeSymbol targetType)
        {
            if (sourceType == targetType)
                return Identity;

            throw new NotImplementedException();

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