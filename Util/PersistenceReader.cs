using System.IO;

namespace FOW
{
    class PersistenceReader : IDataIn
    {
        private Stream stream;
        private BinaryReader reader;

        public PersistenceReader(string path)
        {
            stream = new FileStream(path, FileMode.Open);
            reader = new BinaryReader(stream);
        }

        public PersistenceReader(byte[] bytes)
        {
            stream = new MemoryStream(bytes);
            reader = new BinaryReader(stream);
        }

        public short ReadShort()
        {
            return reader.ReadInt16();
        }

        public ushort ReadUShort()
        {
            return reader.ReadUInt16();
        }

        public int ReadInt()
        {
            return reader.ReadInt32();
        }

        public float ReadFloat()
        {
            return reader.ReadSingle();
        }

        public double ReadDouble()
        {
            return reader.ReadDouble();
        }

        public string ReadString()
        {
            return reader.ReadString();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return reader.ReadSByte();
        }

        public byte[] ReadByteArray()
        {
            int length = reader.ReadInt32();
            return reader.ReadBytes(length);
        }

        public uint ReadUInt()
        {
            return reader.ReadUInt32();
        }

        public char ReadChar()
        {
            return reader.ReadChar();
        }

        public long ReadLong()
        {
            return reader.ReadInt64();
        }

        public void Close()
        {
            stream.Close();
            reader.Close();
        }
    }
}
