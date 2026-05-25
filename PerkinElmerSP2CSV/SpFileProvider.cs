using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace PerkinElmerSP2CSV
{
    /// <summary>
    /// Based on the code for Matlab(R) by Stephen Westlake and Seer Green from Perkin Elmer (2007)
    /// Adapted for OOP and C#.NET by Kutukov Pavel, 2022
    /// </summary>
    public class SpFileProvider : IFileProvider
    {
        private static readonly SpFileProvider _instance = new SpFileProvider();
        private SpFileProvider() { }
        public static SpFileProvider Instance { get => _instance; }
        public string Extension { get; } = ".sp";

        private const Blocks MainBlock = Blocks.DSet2DC1DI;
        private const int DataMemberDataOffset = 4;
        private const int SizeofDouble = 8;

        private static string ReadString(byte[] data)
        {
            try
            {
                int len = BitConverter.ToInt16(data, 0);
                return Encoding.ASCII.GetString(data, 2, len);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Warning: couldn't read a string field due to a bad length value.");
                return null;
            }
        }

        private static void GetSpectrumWrapper(TypedMemberBlock tmb, Spectrum2d sp)
        {
            switch ((Members)tmb.Id)
            {
                case Members.DataSetAbscissaRange:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for X axis range.");
                    sp.StartX = BitConverter.ToDouble(tmb.Data, 0);
                    sp.EndX = BitConverter.ToDouble(tmb.Data, SizeofDouble);
                    break;
                case Members.DataSetInterval:
                    sp.ResolutionX = BitConverter.ToDouble(tmb.Data, 0);
                    break;
                case Members.DataSetNumPoints:
                    sp.PointsY = new double[BitConverter.ToInt32(tmb.Data, 0)];
                    break;
                case Members.DataSetXAxisLabel:
                    sp.LabelX = ReadString(tmb.Data);
                    break;
                case Members.DataSetYAxisLabel:
                    sp.LabelY = ReadString(tmb.Data);
                    break;
                case Members.DataSetData:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdArray)
                        throw new NotSupportedException("Not supported data type for Y data array.");
                    if (sp.PointsY == null) 
                        sp.PointsY = new double[BitConverter.ToInt32(tmb.Data, 0) / SizeofDouble];
                    try
                    {
                        for (int i = 0; i < sp.PointsY.Length; i++)
                        {
                            sp.PointsY[i] = BitConverter.ToDouble(tmb.Data, DataMemberDataOffset + i * SizeofDouble);
                        }
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                    }
                    break;
                case Members.DataSetName:
                    sp.Name = ReadString(tmb.Data);
                    break;
                case Members.DataSetAlias:
                    sp.Alias = ReadString(tmb.Data);
                    break;
                    case Members.DataSetHistoryRecord:
                    var histParser = new HistoryRecordParser(tmb);
                    Console.WriteLine($"   >>> {histParser.GetHistoryRecordAsString()}");
                    break;
                default:
                    Console.WriteLine($"MM >>> ignoring {tmb}");
                    Console.WriteLine($"   >>> {tmb.DumpDataAsHex()}");
                    break;
            }
        }

        private static IEnumerable<TypedMemberBlock> ParseMembers(byte[] data)
        {
            using MemoryStream memoryStream = new MemoryStream(data);
            using BinaryReader binaryReader = new BinaryReader(memoryStream);
            while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
            {
                TypedMemberBlock tmb = null;
                try
                {
                    tmb = new TypedMemberBlock(binaryReader);
                }
                catch (EndOfStreamException)
                {
                    break;
                }
                yield return tmb;
            }
        }

        public IData GetData(string path)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = BlockFile.Load(path).Contents.FirstOrDefault(x => x.Id == (short)MainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(Blocks), MainBlock)} block.");
            var spec = new Spectrum2d();
            foreach (var item in ParseMembers(main.Data))
            {
                GetSpectrumWrapper(item, spec);
            }
            return spec;
        }
    }
}
