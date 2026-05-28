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

            #region CLI Argument Parsing
            switch (args.Length)
            {
                case 1:
                    inputPath = args[0];
                    outputPath = Path.ChangeExtension(inputPath, ".csv");
                    break;
                case 2:
                    inputPath = args[0];
                    outputPath = args[1];
                    break;
                default:
                    Console.WriteLine("Usage: Sp2Csv <input> [<output>]");
                    return;
            }
            #endregion

            try
            {
                var reader = new SpFileTool();
                var spectrum = reader.GetData(inputPath);
                using (var writer = new StreamWriter(outputPath))
                {
                    spectrum.WriteMetaData(writer);
                    spectrum.WriteCsv(writer);
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
