using UnityEngine;

namespace ThreeMG.Helper.ObjectPooling
{
    [System.Serializable]
    public class PoolItem
    {
        public string tag;
        public GameObject prefab;
        public int size;
        public bool dynamic;
        public int maxSize = int.MaxValue;      // Default with no max size limit
    }

    public interface IPoolable
    {
        string PoolTag { get; set; }

        void ReturnToPool();                    //The poolElement should implement IPoolable and call Objpooler.ReturnToPool()
    }
}