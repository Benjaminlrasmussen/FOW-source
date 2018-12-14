
namespace FOW
{
    /// <summary>
    /// An interface for data input.
    /// </summary>
    public interface IDataIn
    {
        /// <exception cref="IOException"></exception>
        byte ReadByte();
        /// <exception cref="IOException"></exception>
        sbyte ReadSByte();
        /// <exception cref="IOException"></exception>
        byte[] ReadByteArray();
        /// <exception cref="IOException"></exception>
        short ReadShort();
        /// <exception cref="IOException"></exception>
        ushort ReadUShort();
        /// <exception cref="IOException"></exception>
        int ReadInt();
        /// <exception cref="IOException"></exception>
        uint ReadUInt();
        /// <exception cref="IOException"></exception>
        float ReadFloat();
        /// <exception cref="IOException"></exception>
        double ReadDouble();
        /// <exception cref="IOException"></exception>
        char ReadChar();
        /// <exception cref="IOException"></exception>
        string ReadString();
        /// <exception cref="IOException"></exception>
        long ReadLong();

        /// <exception cref="IOException"></exception>
        void Close();
    }
}
