using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PerkinElmerSP2CSV
{
    static class Program
    {
        static readonly Dictionary<string, IFileProvider> SupportedProviders = (new IFileProvider[]
        {
            SpFileProvider.Instance
        }).ToDictionary(x => x.Extension, x => x);

        static bool RecursiveOption = false;
        static bool OverwriteOption = false;

        static void Main(string[] args)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Console.WriteLine($"Perkin Elmer CSV {Assembly.GetExecutingAssembly().GetName().Version.ToString()} toolkit started!");
            RecursiveOption = args.Contains("-r");
            OverwriteOption = args.Contains("-o");
            Console.WriteLine($"Recursive folder processing: {RecursiveOption}, Overwrite existing CSV: {OverwriteOption}");
            List<string> files = new List<string>();
            if (args.Length > 0)
            {
                foreach (var item in args)
                {
                    files.AddRange(GetFileOrDir(item));
                }
            }
            else
            {
                files.AddRange(Directory.GetFiles(Environment.CurrentDirectory));
            }
            IEnumerable<string> query = files.Where(x => SupportedProviders.Keys.Contains(Path.GetExtension(x).ToLower()));
            if (!OverwriteOption)
            {
                query = query.Where(x => !File.Exists(GetOutputFilePath(x)));
            }
            files = query.ToList();
            Console.WriteLine($"Info: total files to process = {files.Count}.");
            foreach (var file in files)
            {
                ProcessFile(file);
            }
            Console.WriteLine("Finished.");
        }

        static void ProcessFile(string path)
        {
            try
            {
                BlockFile blockFile = null;
                using (FileStream s = new FileStream(path, FileMode.Open))
                {
                    blockFile = new BlockFile(s);
                }
                Console.WriteLine($"==============================================================");
                Console.WriteLine($">>> BlockFile description: {blockFile.Description}");
                Console.WriteLine($">>> Number of Blocks: {blockFile.Contents.Length}");
                foreach (var block in blockFile.Contents)
                {
                    Console.WriteLine($">>> Block ID={(Members)block.Id} with Block data length {block.Data.Length}");
                }
                Console.WriteLine($"==============================================================");

                using TextWriter textWriter = new StreamWriter(GetOutputFilePath(path));
                IData data = SupportedProviders[Path.GetExtension(path)].GetData(path);
                data?.WriteMetaData(textWriter);
                data?.WriteCsv(textWriter);

                Console.WriteLine(data == null ? $"Warning: no data found in '{path}'." : $"Info: processed file '{path}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: for file '{path}', {Environment.NewLine}\t{ex}");
            }
        }

        static string[] GetFileOrDir(string path)
        {
            if (File.Exists(path))
            {
                return new string[] { path };
            }
            else if (Directory.Exists(path))
            {
                return RecursiveOption ? Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    : Directory.GetFiles(path);
            }
            else
            {
                return new string[] { };
            }
        }

        static string GetOutputFilePath(string inputPath) => inputPath + ".csv";

    }
}
