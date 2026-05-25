using CsvHelper;

namespace PerkinElmerSP2CSV
{
    public interface IData
    {
        public void WriteCsv(CsvWriter w);
        public void WriteMetaData(CsvWriter w);
    }
}
