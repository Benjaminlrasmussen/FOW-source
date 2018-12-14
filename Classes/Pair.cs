
namespace FOW
{
    public struct Pair<L, R>
    {
        public L left;
        public R right;

        public Pair(L left, R right)
        {
            this.left = left;
            this.right = right;
        }
    }
}
