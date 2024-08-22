using UnityEngine;
using System;

namespace ThreeMG.Helper.GridSystem
{
    [Serializable]
    public struct GridConfig
    {
        public GameObject TilePrefab;
        public Vector2Int GridSize;
        public Vector3 TileScale;
        public Vector3 TileOffset;
        public bool IsPrefabLinked;
    }
}