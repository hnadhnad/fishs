using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishStraightPrefab; // Cá đi thẳng
    public GameObject fishWavePrefab;     // Cá lượn sóng
    public GameObject boidPrefab;         // Cá Boid (đàn)

    public float spawnInterval = 2f; // Thời gian spawn giữa các lần
    public float spawnYRange = 4f;   // Cá xuất hiện ngẫu nhiên theo trục Y
    public int boidGroupSize = 5;    // Số lượng cá trong một đàn Boid

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnFish();
        }
    }

    void SpawnFish()
    {
        int rand = Random.Range(0, 3); // chọn ngẫu nhiên 0,1,2
        float yPos = Random.Range(-spawnYRange, spawnYRange);

        if (rand == 0) // Cá đi thẳng
        {
            Instantiate(fishStraightPrefab, new Vector3(-10, yPos, 0), Quaternion.identity);
        }
        else if (rand == 1) // Cá lượn sóng
        {
            Instantiate(fishWavePrefab, new Vector3(-10, yPos, 0), Quaternion.identity);
        }
        else if (rand == 2) // Cá Boid (đàn)
        {
            for (int i = 0; i < boidGroupSize; i++)
            {
                Vector3 spawnPos = new Vector3(-10 + i * 0.3f, yPos + Random.Range(-1f, 1f), 0);
                Instantiate(boidPrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
