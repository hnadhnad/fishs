using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishStraightPrefab;
    public GameObject fishWavePrefab;
    public GameObject boidPrefab;

    public float spawnInterval = 2f;
    public float spawnYRange = 4f;
    public int boidGroupSize = 5;
    public Vector2 spawnXRange = new Vector2(-10f, 10f);

    // size ranges (tùy chỉnh)
    public Vector2 smallSizeRange = new Vector2(0.4f, 1.2f);
    public Vector2 mediumSizeRange = new Vector2(1.3f, 2.3f);
    public Vector2 bigSizeRange = new Vector2(2.4f, 4.0f);

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
        int rand = Random.Range(0, 3);
        float yPos = Random.Range(-spawnYRange, spawnYRange);
        float xPos = spawnXRange.x;

        if (rand == 0)
        {
            var go = Instantiate(fishStraightPrefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
            AssignRandomSize(go);
        }
        else if (rand == 1)
        {
            var go = Instantiate(fishWavePrefab, new Vector3(xPos, yPos, 0), Quaternion.identity);
            AssignRandomSize(go);
        }
        else
        {
            // spawn boid group
            for (int i = 0; i < boidGroupSize; i++)
            {
                Vector3 spawnPos = new Vector3(xPos + i * 0.3f, yPos + Random.Range(-1f, 1f), 0);
                var go = Instantiate(boidPrefab, spawnPos, Quaternion.identity);
                AssignRandomSize(go, smallSizeRange);
            }
        }
    }

    void AssignRandomSize(GameObject go, Vector2? overrideRange = null)
    {
        Fish f = go.GetComponent<Fish>();
        if (f == null) return;

        Vector2 r = overrideRange ?? (Random.value > 0.9f ? bigSizeRange : (Random.value > 0.5f ? mediumSizeRange : smallSizeRange));
        float s = Random.Range(r.x, r.y);
        f.SetSize(s);
    }
}
