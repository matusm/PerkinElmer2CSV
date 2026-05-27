using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    /// <summary>
    /// Overall structure:
    /// Header = PEPE magic header + 40-byte content description string
    /// Contents = an array of blocks with id, size and some binary content
    /// </summary>
    /// <seealso cref="Block"/>
    public class BlockFile
    {
        public string Description { get; }
        public Block[] Contents { get; }

        public BlockFile(FileStream file)
        {
            //Parse header
            byte[] sig = new byte[FileSignature.Length];
            file.Read(sig, 0, sig.Length);
            if (Encoding.ASCII.GetString(sig) != FileSignature)
                throw new NotSupportedException("This is not a Perkin-Elmer block file.");
            sig = new byte[DescriptionRecordLength];
            file.Read(sig, 0, sig.Length);
            Description = Encoding.ASCII.GetString(sig);
            //Read contents
            List<Block> blocks = new List<Block>(); //Todo: some capacity heuristics based on file length?
            try
            {
                using (BinaryReader bReader = new BinaryReader(file))
                {
                    while (bReader.BaseStream.Position < bReader.BaseStream.Length)
                    {
                        blocks.Add(new Block(bReader));
                    }
                }
            }
            catch (EndOfStreamException)
            { }
            Contents = blocks.ToArray();
        }

        public static BlockFile Load(string path)
        {
            BlockFile blockFile;
            using (FileStream s = new FileStream(path, FileMode.Open))
            {
                blockFile = new BlockFile(s);
            }
            return blockFile;
        }

        private const string FileSignature = "PEPE";
        private const int DescriptionRecordLength = 40;
    }
}
