using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantSpawner : MonoBehaviour
{
    [SerializeField]
    public List<PlantSpawningSetting> plantSeeds;

    //允许种植的Layer
    public LayerMask spawningLayer;
    //检测到该Layer不予种植
    public LayerMask avoidLayer;
    public float spawnArea;
    public float raycastDistance = 1f;
    public int maxPlantAmount;

    public bool isTick;

    [Header("Setting")]
    //N次循环都没找到合适的种植点，直接放弃种植
    public int maxCycles = 10;
    //两次种植(成功)间隔时间
    public float plantGap = 30;
    private bool allowPlant = true;

    private List<Plant> plants = new List<Plant>();
    private ContactFilter2D contactFilter;
    private ContactFilter2D avoidFilter;
    

    private void Awake()
    {
        contactFilter.SetLayerMask(spawningLayer);
        contactFilter.useLayerMask = true;
        contactFilter.useTriggers = false;

        avoidFilter.SetLayerMask(avoidLayer);
        avoidFilter.useLayerMask = true;
        avoidFilter.useTriggers = true;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        Tick(Time.deltaTime);
    }

    public void Tick(float deltaTime)
    {
        if (isTick)
        {
            //播种
            if (plantSeeds.Count > 0)
            {
                int flag = 0;
                //spawningLayer.value |= (1 << plantSeeds[0].seed.gameObject.layer);
                for (; plants.Count < maxPlantAmount && allowPlant;)
                {
                    //Note: Prefab的Collider2D.bounds.size是无效的，除非实例化
                    RaycastHit2D[] hit2D = new RaycastHit2D[1];
                    Vector3 origin = transform.position + new Vector3(UnityEngine.Random.Range(-spawnArea / 2, spawnArea / 2), 0, 0);
                    Plant plant = plantSeeds[UnityEngine.Random.Range(0, plantSeeds.Count)].seed;
                    Physics2D.BoxCast(origin, plant.GetComponent<BoxCollider2D>().size, 0, Vector2.down, avoidFilter, hit2D, raycastDistance);
                    if (hit2D[0].collider != null && hit2D[0].collider.gameObject.layer == plant.gameObject.layer)
                    {
                        //Debug.Log("植物尝试种植失败");
                        if (flag > maxCycles)
                        {
                            //Debug.LogWarning("植物种植尝试次数达到最大值，停止种植");
                            return;
                        }
                        flag++;
                        continue;
                    }
                    Physics2D.Raycast(origin, Vector2.down, contactFilter, hit2D, raycastDistance);
                    if (hit2D[0].collider != null)
                    {
                        plants.Add(Instantiate(plant, hit2D[0].point, Quaternion.identity, transform));
                        allowPlant = false;
                        StartCoroutine(PlantGapTimer());
                    }
                }
            }
            //生长
            for (int i = 0; i < plants.Count; i++)
            {
                plants[i].age.isTick = isTick;
            }
        }
    }

    IEnumerator PlantGapTimer()
    {
        yield return new WaitForSeconds(plantGap);
        allowPlant = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(spawnArea, 0.4f, 0));
    }
}

[Serializable]
public class PlantSpawningSetting
{
    public Plant seed;
    public int spawnWeight;
}
