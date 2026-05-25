using System.IO;

namespace PerkinElmerSP2CSV
{
    public interface IData
    {
        public void WriteCsv(TextWriter w);
        public void WriteMetaData(TextWriter w);
    }
}
