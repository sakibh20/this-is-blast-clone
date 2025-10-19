using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private Projectile prefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private Transform projectileParent;

    private readonly List<Projectile> _pool = new List<Projectile>();

    public static ObjectPool Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        for (int i = 0; i < poolSize; i++)
        {
            GenerateItem();
        }
    }

    public Projectile GetPooledObject()
    {
        foreach (Projectile obj in _pool)
        {
            if (!obj.gameObject.activeInHierarchy)
                return obj;
        }

        return GenerateItem();
    }

    private Projectile GenerateItem()
    {
        Projectile obj = Instantiate(prefab, projectileParent);
        obj.objectPool = this;
        obj.gameObject.SetActive(false);
        _pool.Add(obj);

        return obj;
    }
    
    public void ReturnToPool(Projectile projectile)
    {
        projectile.gameObject.SetActive(false);
    }
}