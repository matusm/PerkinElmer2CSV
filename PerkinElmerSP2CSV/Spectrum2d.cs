using CsvHelper;
using At.Matus.MetaData;
using System.Collections.Immutable;
using System;
using System.Collections.Generic;
using System.Linq;

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
            Dictionary<string, string> metaDataRecords = MetaData.Records;
            Dictionary<string, string> sortedDict = metaDataRecords.OrderBy(kv => kv.Key)
                     .ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var item in sortedDict)
            {
                w.WriteField(item.Key);
                w.WriteField(item.Value);
                w.NextRecord();
            }
        }

        public void WriteCsv(CsvWriter w)
        {
            Array.Sort(Points);
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
