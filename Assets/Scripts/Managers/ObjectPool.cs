using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private readonly GameObject _prefab;
    private readonly Transform _root;
    private readonly HashSet<GameObject> _activeObjects = new();
    private readonly Stack<GameObject> _inactiveObjects = new();

    public ObjectPool(GameObject prefab, int count, Transform parent)
    {
        _prefab = prefab;
        _root = new GameObject($"{prefab.name}_Pool").transform;
        _root.SetParent(parent);

        for (int i = 0; i < count; i++)
        {
            var go = Create();
            Deactivate(go, false);
        }
    }

    public GameObject Get(Transform parent = null)
    {
        GameObject go = null;

        while (_inactiveObjects.Count > 0)
        {
            go = _inactiveObjects.Pop();
            if (go != null)
            {
                break;
            }
        }

        if (go == null)
        {
            go = Create();
        }

        if (parent != null)
        {
            go.transform.SetParent(parent);
        }

        go.SetActive(true);

        if (go.TryGetComponent<IPoolable>(out var poolable))
        {
            poolable.OnSpawn();
        }

        _activeObjects.Add(go);

        return go;
    }

    public bool Return(GameObject go)
    {
        if (!_activeObjects.Remove(go))
        {
            return false;
        }

        Deactivate(go);

        return true;
    }

    public void ReturnAll()
    {
        foreach (var go in _activeObjects)
        {
            Deactivate(go);
        }

        _activeObjects.Clear();
    }

    public void Clear()
    {
        foreach (var go in _activeObjects)
        {
            if (go != null)
            {
                Object.Destroy(go);
            }
        }

        foreach (var go in _inactiveObjects)
        {
            if (go != null)
            {
                Object.Destroy(go);
            }
        }

        _activeObjects.Clear();
        _inactiveObjects.Clear();
    }

    public void Dispose()
    {
        Clear();

        if (_root != null)
        {
            Object.Destroy(_root.gameObject);
        }
    }

    private GameObject Create()
    {
        var go = Object.Instantiate(_prefab, _root);
        go.name = _prefab.name;
        return go;
    }

    private void Deactivate(GameObject go, bool callDespawn = true)
    {
        if (go != null)
        {
            if (callDespawn && go.TryGetComponent<IPoolable>(out var poolable))
            {
                poolable.OnDespawn();
            }

            go.SetActive(false);
            go.transform.SetParent(_root);
            _inactiveObjects.Push(go);
        }
    }
}
