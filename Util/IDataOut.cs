
namespace FOW
{
    /// <summary>
    /// An interface for data output.
    /// </summary>
    public interface IDataOut
    {
        /// <exception cref="IOException"></exception>
        void Write(byte b);
        /// <exception cref="IOException"></exception>
        void Write(sbyte sb);
        /// <exception cref="IOException"></exception>
        void Write(byte[] bArr);
        /// <exception cref="IOException"></exception>
        void Write(short s);
        /// <exception cref="IOException"></exception>
        void Write(ushort s);
        /// <exception cref="IOException"></exception>
        void Write(int i);
        /// <exception cref="IOException"></exception>
        void Write(uint ui);
        /// <exception cref="IOException"></exception>
        void Write(float f);
        /// <exception cref="IOException"></exception>
        void Write(double d);
        /// <exception cref="IOException"></exception>
        void Write(char c);
        /// <exception cref="IOException"></exception>
        void Write(string s);
        /// <exception cref="IOException"></exception>
        void Write(long l);
        /// <exception cref="IOException"></exception>
        void Close();
    }
}