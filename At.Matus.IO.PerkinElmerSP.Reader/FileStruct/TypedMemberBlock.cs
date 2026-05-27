using System.IO;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    /// <summary>
    /// ID (int16)
    /// Len (int32)
    /// TypeCode (int16) -- only DataSetDataMember?
    /// Data (len)
    /// </summary>
    public class TypedMemberBlock : Block
    {
        public short TypeCode { get; }

        public TypedMemberBlock(BinaryReader file) : base(file.ReadInt16())
        {
            int len = file.ReadInt32();
            TypeCode = file.ReadInt16();
            Data = file.ReadBytes(len - 2);
        }

        public string DumpDataAsHex()
        {
            string dataString = string.Empty;
            foreach (byte b in Data)
            {
                dataString += $"{b:X2} ";
            }
            return dataString.TrimEnd();
        }

        public override string ToString() => $"TypedMemberBlock: Id={(Members)Id} dataLength={Data.Length} TypeCode={(TypeCodes)TypeCode}";
    }

}
