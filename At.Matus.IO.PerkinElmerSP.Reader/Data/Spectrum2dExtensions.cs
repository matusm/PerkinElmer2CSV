using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    public static class Spectrum2dExtensions
    {
        public static void WriteMetaDataAsComments(this Spectrum2d spectrum, TextWriter w)
        {
            foreach (var item in spectrum.MetaData.Records)
            {
                w.WriteLine($"# {item.Key} = {item.Value}");
            }
        }

        public static void WriteCsv(this Spectrum2d spectrum, TextWriter w)
        {
            Array.Sort(spectrum.Points);
            //Header
            w.WriteLine($"{spectrum.LabelX},{spectrum.LabelY}");
            foreach (var point in spectrum.Points)
            {
                w.WriteLine($"{point.X},{point.Y}");
            }
        }

        public static void WriteMetaDataAsJson(this Spectrum2d spectrum, TextWriter w)
        {
            var json = DictionaryToPrettyJson(spectrum.MetaData.Records);
            w.Write(json);
        }

        // this should be moved to a more general utility class if we need to use it in other places
        private static string DictionaryToPrettyJson(Dictionary<string, string> dict)
        {
            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            };
            return JsonSerializer.Serialize(dict ?? new Dictionary<string, string>(), options);
        }
    }
}
