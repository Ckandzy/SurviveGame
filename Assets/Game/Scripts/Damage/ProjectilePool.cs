using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gamekit2D;

public class ProjectilePool : ObjectPool<ProjectilePool, ProjectileObject, Projectile.ProjectData>
{
    static protected Dictionary<GameObject, ProjectilePool> s_PoolInstances = new Dictionary<GameObject, ProjectilePool>();

    private void Awake()
    {
        //This allow to make Pool manually added in the scene still automatically findable & usable
        if (prefab != null && !s_PoolInstances.ContainsKey(prefab))
            s_PoolInstances.Add(prefab, this);
    }

    private void OnDestroy()
    {
        s_PoolInstances.Remove(prefab);
    }

    //initialPoolCount is only used when the objectpool don't exist
    static public ProjectilePool GetObjectPool(GameObject prefab, int initialPoolCount = 10)
    {
        ProjectilePool objPool = null;
        if (!s_PoolInstances.TryGetValue(prefab, out objPool))
        {
            GameObject obj = new GameObject(prefab.name + "_Pool");
            objPool = obj.AddComponent<ProjectilePool>();
            objPool.prefab = prefab;
            objPool.initialPoolCount = initialPoolCount;

            s_PoolInstances[prefab] = objPool;
        }

        return objPool;
    }
}

public class ProjectileObject : PoolObject<ProjectilePool, ProjectileObject, Projectile.ProjectData>
{
    public Transform transform;
    public Rigidbody2D rigidbody2D;
    public SpriteRenderer spriteRenderer;
    public Projectile projectile;

    protected override void SetReferences()
    {
        transform = instance.transform;
        rigidbody2D = instance.GetComponent<Rigidbody2D>();
        spriteRenderer = instance.GetComponent<SpriteRenderer>();
        projectile = instance.GetComponent<Projectile>();
        projectile.projectilePoolObject = this;
        //bullet.mainCamera = Object.FindObjectOfType<Camera>();
    }

    public override void WakeUp(Projectile.ProjectData projectData)
    {
        transform.rotation = Quaternion.identity;
        transform.position = projectData.shootOrigin;
        projectile.projectData = projectData;
        instance.SetActive(true);
    }

    public override void Sleep()
    {
        instance.SetActive(false);
    }
}
