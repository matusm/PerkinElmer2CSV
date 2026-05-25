using At.Matus.MetaData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

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

        public void WriteMetaData(TextWriter w)
        {
            Dictionary<string, string> metaDataRecords = MetaData.Records;
            Dictionary<string, string> sortedDict = metaDataRecords.OrderBy(kv => kv.Key)
                     .ToDictionary(kv => kv.Key, kv => kv.Value);
            foreach (var item in sortedDict)
            {
                w.WriteLine($"# {item.Key} = {item.Value}");
            }
        }

        public void WriteCsv(TextWriter w)
        {
            Array.Sort(Points);
            //Header
            w.WriteLine($"{LabelX},{LabelY}");
            //Rows
            foreach (var item in Points)
            {
                w.WriteLine($"{item.X},{item.Y}");
            }
        }
    }
}
