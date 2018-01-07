using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private TileScript prefab;

    private List<TileScript> availableObjects = new List<TileScript>();

    public TileScript GetObject(TileType type)
    {
        TileScript obj;
        int lastAvailableIndex = availableObjects.Count - 1;
        if (lastAvailableIndex >= 0)
        {
            obj = availableObjects[lastAvailableIndex];
            availableObjects.RemoveAt(lastAvailableIndex);
            obj.tileType = type;
            obj.gameObject.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab);
            obj.transform.SetParent(transform, false);
            obj.Pool = this;
        }
        return obj;
    }

    public void AddObject(TileScript obj)
    {
        obj.transform.SetParent(transform);
        obj.gameObject.SetActive(false);
        availableObjects.Add(obj);
    }

    public static ObjectPool GetPool(TileScript prefab)
    {
        GameObject obj;
        ObjectPool pool;
        if (Application.isEditor)
        {
            obj = GameObject.Find(prefab.name + " Pool");
            if (obj)
            {
                pool = obj.GetComponent<ObjectPool>();
                if (pool)
                {
                    return pool;
                }
            }
        }
        obj = new GameObject(prefab.name + " Pool");
        pool = obj.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        return pool;
    }
}
