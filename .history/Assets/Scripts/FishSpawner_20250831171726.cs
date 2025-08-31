using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Spawner: spawn cá từ danh sách prefab.
/// Size của cá được đặt sẵn trong prefab (Inspector).
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnInfo
    {
        public GameObject prefab; // prefab cá (đặt size sẵn trong prefab)
        public int count = 1;     // số lượng spawn
    }

    [Header("Spawn Settings")]
    public List<SpawnInfo> spawnList = new List<SpawnInfo>();
    public Vector2 spawnAreaMin = new Vector2(-8, -4);
    public Vector2 spawnAreaMax = new Vector2(8, 4);

    void Start()
    {
        foreach (var info in spawnList)
        {
            for (int i = 0; i < info.count; i++)
            {
                Vector2 pos = new Vector2(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y)
                );

                Instantiate(info.prefab, pos, Quaternion.identity, transform);
            }
        }
    }
}
