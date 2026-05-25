namespace PerkinElmerSP2CSV
{
    public interface IFileProvider
    {
        public string Extension { get; }
        public IData GetData(string path);
    }
}
