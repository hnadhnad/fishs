using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public GameObject fishStraightPrefab;
    public GameObject fishWavePrefab;
    public GameObject boidPrefab;

    public float spawnInterval = 2f;

    [Header("Spawn Ranges")]
    public Vector2 spawnXRange = new Vector2(-19f, 19f); // biên trái & phải
    public float spawnYRange = 4.5f;

    [Header("Boid Settings")]
    public int boidGroupSize = 5;

    [Header("Fish Size Ranges")]
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
        float xPos = Random.Range(spawnXRange.x, spawnXRange.y);
        float yPos = Random.Range(-spawnYRange, spawnYRange);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        // Xác định hướng bơi dựa vào spawn bên trái hay phải
        int dir = (xPos < 0) ? 1 : -1;

        if (rand == 0)
        {
            var go = Instantiate(fishStraightPrefab, spawnPos, Quaternion.identity);
            SetupDirection(go, dir);
            AssignRandomSize(go);
        }
        else if (rand == 1)
        {
            var go = Instantiate(fishWavePrefab, spawnPos, Quaternion.identity);
            SetupDirection(go, dir);
            AssignRandomSize(go);
        }
        else
        {
            for (int i = 0; i < boidGroupSize; i++)
            {
                Vector3 offset = new Vector3(i * 0.3f * dir, Random.Range(-0.5f, 0.5f), 0);
                var go = Instantiate(boidPrefab, spawnPos + offset, Quaternion.identity);
                SetupDirection(go, dir);
                AssignRandomSize(go, smallSizeRange);
            }
        }
    }

    void SetupDirection(GameObject go, int dir)
    {
        // Nếu có FishStraight thì chỉnh hướng
        FishStraight straight = go.GetComponent<FishStraight>();
        if (straight != null)
            straight.direction = Vector2.right * dir;

        // Nếu có FishWave thì đảo speed
        FishWave wave = go.GetComponent<FishWave>();
        if (wave != null)
            wave.speed *= dir;

        // Nếu là Boid thì chỉnh velocity ban đầu
        Boid boid = go.GetComponent<Boid>();
        if (boid != null)
            boid.velocity = new Vector2(dir, 0) * boid.speed;

        // Quay mặt cá cho đúng
        go.transform.localScale = new Vector3(dir * Mathf.Abs(go.transform.localScale.x),
                                              go.transform.localScale.y,
                                              go.transform.localScale.z);
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
