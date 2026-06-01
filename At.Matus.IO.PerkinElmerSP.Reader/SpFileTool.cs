using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    /// <summary>
    /// Based on the code for Matlab(R) by Stephen Westlake and Seer Green from Perkin Elmer (2007)
    /// Adapted for OOP and C#.NET by Kutukov Pavel, 2022
    /// </summary>
    public class SpFileTool
    {
        public string Extension => ".sp";

        public Spectrum2d GetData(string path)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            Block main = BlockFile.Load(path).Contents.FirstOrDefault(x => x.Id == (short)mainBlock);
            if (main == null)
                throw new NotSupportedException($"This SP file doesn't contain a {Enum.GetName(typeof(Blocks), mainBlock)} block.");
            var spec = new Spectrum2d();
            foreach (var item in ParseMembers(main.Data))
            {
                GetSpectrumWrapper(item, spec);
            }
            spec.MetaData.AddRecord("SourceFile", Path.GetFileName(path));
            return spec;
        }

        private static string ReadString(byte[] data)
        {
            try
            {
                int len = BitConverter.ToInt16(data, 0);
                return Encoding.Default.GetString(data, 2, len);
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
                    sp.EndX = BitConverter.ToDouble(tmb.Data, sizeOfDouble);
                    sp.MetaData.AddRecord("DataSetAbscissaRange", $"{sp.EndX} {sp.StartX}");
                    break;
                case Members.DataSetInterval:
                    sp.ResolutionX = BitConverter.ToDouble(tmb.Data, 0);
                    sp.MetaData.AddRecord("DataSetInterval", $"{sp.ResolutionX}");
                    break;
                case Members.DataSetNumPoints:
                    // there might be an inconsitency with the actual length of the data array!
                    var numPoints = BitConverter.ToInt32(tmb.Data, 0);
                    sp.Points = new Point2d[numPoints];
                    sp.MetaData.AddRecord("DataSetNumPoints", $"{numPoints}");
                    break;
                case Members.DataSetXAxisLabel:
                    sp.LabelX = ReadString(tmb.Data);
                    sp.MetaData.AddRecord("DataSetXAxisLabel", $"{sp.LabelX}");
                    break;
                case Members.DataSetYAxisLabel:
                    sp.LabelY = ReadString(tmb.Data);
                    sp.MetaData.AddRecord("DataSetYAxisLabel", $"{sp.LabelY}");
                    break;
                case Members.DataSetData:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdArray)
                        throw new NotSupportedException("Not supported data type for Y data array.");
                    if (sp.Points == null)
                        sp.Points = new Point2d[BitConverter.ToInt32(tmb.Data, 0) / sizeOfDouble];
                    try
                    {
                        for (int i = 0; i < sp.Points.Length; i++)
                        {
                            double y = BitConverter.ToDouble(tmb.Data, dataMemberDataOffset + i * sizeOfDouble);
                            sp.Points[i] = new Point2d { X = sp.StartX + i * sp.ResolutionX, Y = y };
                        }
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Warning: an unexpected end of data member block has been encountered.");
                    }
                    break;
                case Members.DataSetName:
                    sp.MetaData.AddRecord("DataSetName", $"{ReadString(tmb.Data)}");
                    break;
                case Members.DataSetAlias:
                    sp.MetaData.AddRecord("DataSetAlias", $"{ReadString(tmb.Data)}");
                    break;
                case Members.DataSetHistoryRecord:
                    var histParser = new HistoryRecordParser(tmb);
                    var histRecordsAsDictionary = histParser.GetHistoryRecordsAsDictionary();
                    sp.MetaData.AddRecords(histRecordsAsDictionary);
                    break;
                case Members.DataSetDataType:
                    sp.MetaData.AddRecord("DataSetDataType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetFileType:
                    sp.MetaData.AddRecord("DataSetFileType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetSamplingMethod:
                    sp.MetaData.AddRecord("DataSetSamplingMethod", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetXAxisUnitType:
                    sp.MetaData.AddRecord("DataSetXAxisUnitType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetYAxisUnitType:
                    sp.MetaData.AddRecord("DataSetYAxisUnitType", $"{BitConverter.ToInt16(tmb.Data, 0)}");
                    break;
                case Members.DataSetChecksum:
                    sp.MetaData.AddRecord("DataSetChecksum", $"{BitConverter.ToInt32(tmb.Data, 0)}");
                    break;
                case Members.DataSetOrdinateRange:
                    if (tmb.TypeCode != (short)TypeCodes.CvCoOrdRange)
                        throw new NotSupportedException("Not supported data type for DataSetOrdinateRange.");
                    var f1 = BitConverter.ToDouble(tmb.Data, 0);
                    var f2 = BitConverter.ToDouble(tmb.Data, sizeOfDouble);
                    sp.MetaData.AddRecord("DataSetOrdinateRange", $"{f1} {f2}");
                    break;
                default:
                    sp.MetaData.AddRecord($"XXX_{(Members)tmb.Id}", $"{tmb.DumpDataAsHex()}");
                    break;
            }
        }

        private static IEnumerable<TypedMemberBlock> ParseMembers(byte[] data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
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
        }

        private const Blocks mainBlock = Blocks.DSet2DC1DI;
        private const int dataMemberDataOffset = 4;
        private const int sizeOfDouble = 8;
    }
}
