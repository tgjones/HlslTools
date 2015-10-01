namespace HlslTools.Text
{
    public interface IIncludeFileSystem
    {
        SourceText GetInclude(string path);
    }
}