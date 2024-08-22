using System.Collections.Generic;
using UnityEngine;

namespace ThreeMG.Helper.ObjectPooling
{
    public class ObjectPooler : MonoBehaviour
    {
        public bool NonDestroyable = true;
        public static ObjectPooler Instance { get; private set; }
        public List<PoolItem> itemsToPool;
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        private Dictionary<string, PoolItem> poolItemDictionary;
        private static int itemCount = 0;
        private static string poolItemName = "PoolItem";    //Change the item name here.


        void Awake()
        {
            SetAsSingleton();
            InitializePools();
        }

        private void SetAsSingleton()
        {
            if (Instance == null)
            {
                Instance = this;
                if (NonDestroyable)
                {
                    DontDestroyOnLoad(gameObject);
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            poolItemDictionary = new Dictionary<string, PoolItem>();

            foreach (var item in itemsToPool)
            {
                InitializePool(item);

            }
        }

        private void InitializePool(PoolItem item)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            poolItemDictionary[item.tag] = item;

            for (int i = 0; i < item.size; i++)
            {
                GameObject obj = CreateNewObject(item.prefab, false, item.tag);
                objectPool.Enqueue(obj);
            }

            poolDictionary[item.tag] = objectPool;
        }


        private GameObject CreateNewObject(GameObject prefab, bool setActive, string tag)
        {
            GameObject obj = Instantiate(prefab, this.transform);
            obj.name = poolItemName + " " + itemCount.ToString();
            itemCount++;
            
            IPoolable poolable = obj.GetComponent<IPoolable>();
            if (poolable != null)
            {
                poolable.PoolTag = tag;
            }
            obj.SetActive(setActive);
            return obj;
        }

        #region POOLING Functions

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " does not exist.");
                return null;
            }

            Queue<GameObject> poolQueue = poolDictionary[tag];
            if (poolQueue.Count == 0 && poolItemDictionary[tag].dynamic)
            {
                if (poolQueue.Count < poolItemDictionary[tag].maxSize)
                {
                    poolQueue.Enqueue(CreateNewObject(poolItemDictionary[tag].prefab, false, tag));
                }
                else
                {
                    Debug.LogWarning("Reached maximum pool size for " + tag);
                    return null;
                }
            }

            GameObject objectToSpawn = poolQueue.Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // poolQueue.Enqueue(objectToSpawn); // Re-enqueue for future use
            return objectToSpawn;
        }

        public void ReturnToPool(GameObject returnableObj)
        {
            IPoolable poolable = returnableObj.GetComponent<IPoolable>();

            if (poolable != null)
            {
                if (poolDictionary.ContainsKey(poolable.PoolTag))
                {
                    Debug.Log("Returned to pool" + poolDictionary[poolable.PoolTag].Count);
                    poolDictionary[poolable.PoolTag].Enqueue(returnableObj);
                    returnableObj.transform.parent = this.transform;
                    returnableObj.SetActive(false);
                }
                else
                {
                    Debug.LogError($"{poolable.PoolTag} pool is not available.");
                }
            }
            else
            {
                Debug.LogError($"{returnableObj.name} is not IPoolable");
            }
        }

        #endregion
    }
}

// public class ExampleUsage : MonoBehaviour
// {
//     public Transform bulletSpawnPoint;

//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.Space))
//         {
//             ObjectPooler.Instance.SpawnFromPool("bullet", bulletSpawnPoint.position, Quaternion.identity);
//         }
//     }
// }