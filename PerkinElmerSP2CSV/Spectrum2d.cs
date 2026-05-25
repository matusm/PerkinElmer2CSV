using CsvHelper;
using At.Matus.MetaData;

namespace PerkinElmerSP2CSV
{
    public class Spectrum2d : IData
    {
        public MeasurementMetaData MetaData { get; set; } = new MeasurementMetaData();
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double ResolutionX { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public Point2d[] Points { get; set; }
        //public double[] PointsY { get; set; }

        public void WriteMetaData(CsvWriter w)
        {
            w.WriteField("MetaDataKey");
            w.WriteField("MetaDataValue");
            w.NextRecord();
            foreach (var item in MetaData.Records)
            {
                w.WriteField(item.Key);
                w.WriteField(item.Value);
                w.NextRecord();
            }
        }

        public void WriteCsv(CsvWriter w)
        {
            //Header
            w.WriteField(LabelX);
            w.WriteField(LabelY);
            w.NextRecord();
            //Rows
            foreach (var item in Points)
            {
                w.WriteField(item.X);
                w.WriteField(item.Y);
                w.NextRecord();
            }
        }
    }
}
