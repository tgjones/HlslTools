namespace ShaderTools.Hlsl.Text
{
    public interface IIncludeFileSystem
    {
        SourceText GetInclude(string path);
    }
}