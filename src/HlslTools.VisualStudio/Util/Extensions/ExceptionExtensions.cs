using System;

namespace HlslTools.VisualStudio.Util.Extensions
{
    internal static class ExceptionExtensions
    {
        public static bool IsCritical(this Exception e)
        {
            if (e is AccessViolationException
                || e is StackOverflowException
                || e is OutOfMemoryException
                || e is BadImageFormatException
                || e is AppDomainUnloadedException)
            {
                return true;
            }

            return false;
        }
    }
}