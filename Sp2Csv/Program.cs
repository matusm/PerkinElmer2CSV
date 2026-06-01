using At.Matus.IO.PerkinElmerSP.Reader;
using System;
using System.Globalization;
using System.IO;

namespace Sp2Csv
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            string inputPath = string.Empty;
            string outputPath = string.Empty;
            string jsonPath = string.Empty;

            #region CLI Argument Parsing
            switch (args.Length)
            {
                case 1:
                    inputPath = args[0];
                    outputPath = Path.ChangeExtension(inputPath, ".csv");
                    jsonPath = Path.ChangeExtension(inputPath, ".json");
                    break;
                case 2:
                    inputPath = args[0];
                    outputPath = args[1];
                    jsonPath = Path.ChangeExtension(outputPath, ".json");
                    break;
                default:
                    Console.WriteLine("Usage: Sp2Csv <input> [<output>]");
                    return;
            }
            #endregion

            try
            {
                var reader = new SpFileTool();
                reader.IncludUnknownBlocksInMetaData = false; // for reverse engineering purposes, set to true to include all unknown blocks in the metadata
                var spectrum = reader.GetData(inputPath);
                using (var writer = new StreamWriter(outputPath, false, System.Text.Encoding.UTF8))
                {
                    spectrum.WriteMetaDataAsComments(writer);
                    spectrum.WriteCsv(writer);
                }
                using (var writer = new StreamWriter(jsonPath, false, System.Text.Encoding.UTF8))
                {
                    spectrum.WriteMetaDataAsJson(writer);
                }
                Console.WriteLine($"Successfully converted {inputPath} to {outputPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
