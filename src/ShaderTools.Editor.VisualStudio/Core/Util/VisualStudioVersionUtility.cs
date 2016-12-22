namespace ShaderTools.Editor.VisualStudio.Core.Util
{
    internal static class VisualStudioVersionUtility
    {
        public static VisualStudioVersion FromDteVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                return VisualStudioVersion.Unknown;

            var parts = version.Split('.');
            if (parts.Length == 0)
                return VisualStudioVersion.Unknown;

            switch (parts[0])
            {
                case "11":
                    return VisualStudioVersion.Vs2012;
                case "12":
                    return VisualStudioVersion.Vs2013;
                case "14":
                    return VisualStudioVersion.Vs2015;
                default:
                    return VisualStudioVersion.Unknown;
            }
        }
    }
}