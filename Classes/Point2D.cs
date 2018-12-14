
namespace FOW
{
    public struct Point2D : ISerializable
    {
        public byte x;
        public byte y;

        public Point2D(byte x, byte y)
        {
            this.x = x;
            this.y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Point2D))
            {
                return false;
            }

            var d = (Point2D)obj;
            return x == d.x &&
                   y == d.y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public void Serialize(IDataOut dataOut)
        {
            dataOut.Write(x);
            dataOut.Write(y);
        }

        public static Point2D DeSerialize(IDataIn dataIn)
        {
            Point2D instance = new Point2D();
            instance.x = dataIn.ReadByte();
            instance.y = dataIn.ReadByte();
            return instance;
        }
    }

}