using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly T _prefab;
    private readonly Transform _root;
    private readonly HashSet<T> _activeObjects = new();
    private readonly Stack<T> _inactiveObjects = new();

    public ObjectPool(T prefab, int count, Transform parent = null)
    {
        _prefab = prefab;
        _root = new GameObject($"{typeof(T).Name}_Pool").transform;
        _root.SetParent(parent);

        for (int i = 0; i < count; i++)
        {
            var obj = Create();
            Deactivate(obj);
        }
    }

    public T Get(Transform parent = null)
    {
        T obj = null;

        while (_inactiveObjects.Count > 0)
        {
            obj = _inactiveObjects.Pop();
            if (obj != null)
            {
                break;
            }
        }

        if (obj == null)
        {
            obj = Create();
        }

        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }

        obj.gameObject.SetActive(true);
        (obj as IPoolable)?.OnSpawn();
        _activeObjects.Add(obj);

        return obj;
    }

    public bool Return(T obj)
    {
        if (!_activeObjects.Remove(obj))
        {
            return false;
        }

        Deactivate(obj);

        return true;
    }

    public void ReturnAll()
    {
        foreach (var obj in _activeObjects)
        {
            Deactivate(obj);
        }

        _activeObjects.Clear();
    }

    public void Clear()
    {
        foreach (var obj in _activeObjects)
        {
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
            }
        }

        foreach (var obj in _inactiveObjects)
        {
            if (obj != null)
            {
                Object.Destroy(obj.gameObject);
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

    private T Create()
    {
        var obj = Object.Instantiate(_prefab, _root);
        obj.name = _prefab.name;
        return obj;
    }

    private void Deactivate(T obj)
    {
        if (obj != null)
        {
            (obj as IPoolable)?.OnDespawn();
            obj.gameObject.SetActive(false);
            obj.transform.SetParent(_root);
            _inactiveObjects.Push(obj);
        }
    }
}
