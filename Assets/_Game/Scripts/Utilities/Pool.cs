using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class Pool
{

    private class Pooler : MonoBehaviour
    {
        public GameObject _Prefab;
    }

    private static Pool _pool;

    public static Pool GetPool()
    {
        if (_pool == null)
            _pool = new Pool();

        return _pool;
    }

    private Pool()
    {
        _myObjects = new Dictionary<GameObject, Queue<GameObject>>();
    }


    //Class functions
    private Dictionary<GameObject, Queue<GameObject>> _myObjects;

    public GameObject GetObject(GameObject prefab)
    {
        if (prefab == null)
            return null;

        GameObject obj = null;

        if (!_myObjects.ContainsKey(prefab))
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            _myObjects.Add(prefab, newQueue);
        }
        
        if (_myObjects[prefab].Count == 0)
        {
            obj = CreateObject(prefab);
        } else
        {
            obj = _myObjects[prefab].Dequeue();
            if(obj == null)
                obj = CreateObject(prefab);
            obj.SetActive(false);
        }

        return obj;
    }

    public void ReturnObject(GameObject go)
    {
        if (go == null)
            return;
        
        go.SetActive(false);

        Pool.Pooler pooler = go.GetComponent<Pooler>();
        if(pooler == null || pooler._Prefab == null)
        {
            GameObject.Destroy(go);
            return;
        }

        if (!_myObjects.ContainsKey(pooler._Prefab))
        {
            Queue<GameObject> newQueue = new Queue<GameObject>();
            _myObjects.Add(pooler._Prefab, newQueue);
        } else
        {
            _myObjects[pooler._Prefab].Enqueue(go);
        }
    }

    private GameObject CreateObject(GameObject prefab)
    {
        GameObject obj = GameObject.Instantiate(prefab);

        Pool.Pooler pooler = obj.GetComponent<Pooler>();
        if(pooler == null)
        {
            pooler = obj.AddComponent<Pooler>();
            pooler._Prefab = prefab;
        }

        obj.SetActive(false);

        return obj;
    }
}
