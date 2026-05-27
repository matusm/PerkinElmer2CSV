using System.IO;

namespace At.Matus.IO.PerkinElmerSP.Reader
{
    /// <summary>
    /// Each block contains fields:
    /// Id (int16), Length (int32), [for "member"-blocks only: innerCode (int16) = data type], data (arbitrary).
    /// For *.SP files "member-blocks" are considered as data of "wrapper-blocks"
    /// </summary>
    /// <seealso cref="TypedMemberBlock"/>
    public class Block
    {
        public short Id { get; }
        public byte[] Data { get; protected set; }

        protected Block(short id)
        {
            Id = id;
        }

        public Block(BinaryReader file)
        {
            Id = file.ReadInt16();
            int len = file.ReadInt32();
            Data = file.ReadBytes(len);
            if (Data.Length < len) throw new EndOfStreamException();
        }

        public Block(short id, byte[] data)
        {
            Id = id;
            Data = data;
        }
    }
}
