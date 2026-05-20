using UnityEngine;

namespace PPCorps
{
    [System.Serializable]
    public struct GridPosition
    {
        public int col;

        public GridPosition(int col)
        {
            this.col = col;
        }

        public static int Distance(GridPosition a, GridPosition b)
            => Mathf.Abs(a.col - b.col);

        public static GridPosition operator +(GridPosition a, int offset)
            => new GridPosition(a.col + offset);

        public static GridPosition operator -(GridPosition a, int offset)
            => new GridPosition(a.col - offset);

        public static bool operator ==(GridPosition a, GridPosition b)
            => a.col == b.col;

        public static bool operator !=(GridPosition a, GridPosition b)
            => a.col != b.col;

        public override bool Equals(object obj)
            => obj is GridPosition other && this == other;

        public override int GetHashCode() => col;
    }
}
