using System.IO;

namespace FOW
{
    public class PersistenceWriter : IDataOut
    {
        private FileStream stream;
        private BinaryWriter writer;

        public PersistenceWriter(string path)
        {
            stream = new FileStream(path, FileMode.Create);
            writer = new BinaryWriter(stream);
        }

        public void Write(byte b)
        {
            writer.Write(b);
        }

        public void Write(sbyte sb)
        {
            writer.Write(sb);
        }

        public void Write(byte[] bArr)
        {
            writer.Write(bArr);
        }

        public void Write(short s)
        {
            writer.Write(s);
        }

        public void Write(ushort s)
        {
            writer.Write(s);
        }

        public void Write(int i)
        {
            writer.Write(i);
        }

        public void Write(uint ui)
        {
            writer.Write(ui);
        }

        public void Write(float f)
        {
            writer.Write(f);
        }

        public void Write(double d)
        {
            writer.Write(d);
        }

        public void Write(char c)
        {
            writer.Write(c);
        }

        public void Write(string s)
        {
            writer.Write(s);
        }

        public void Write(long l)
        {
            writer.Write(l);
        }

        public void Close()
        {
            stream.Close();
            writer.Close();
        }
    }
}
