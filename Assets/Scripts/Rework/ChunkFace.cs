using UnityEngine;

namespace Prism.Rework
{
    public struct ChunkFace
    {
        public Vector3 center;
        public int direction;
        public Vector2Int tileUp;
        public Vector2Int tileDown;
        public Vector2Int tileBorders;
    }

    /* direction :
     * 
     * 0 = up
     * 1 = down
     * 2 = left
     * 3 = right
     * 4 = front
     * 5 = back
     */
}
