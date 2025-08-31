using UnityEngine;

/// <summary>
/// Spawner sinh cá (thẳng, lượn sóng, đàn boid) và tảo.
/// Mỗi prefab cá tự đặt size sẵn trong Inspector, không random size nữa.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject fishStraightPrefab;
    public GameObject fishWavePrefab;
    public GameObject boidPrefab;

    [Header("Spawn Settings")]
    public float spawnInterval = 2f;
    public float spawnLeftX = -22f;
    public float spawnRightX = 22f;
    public float spawnYRange = 12f;

    [Header("Spawn Chances (tổng nên = 1.0 hoặc 100%)")]
    [Range(0f, 1f)] public float straightChance = 0.4f;
    [Range(0f, 1f)] public float waveChance = 0.4f;
    [Range(0f, 1f)] public float boidChance = 0.2f;


    [Header("Boid Settings")]
    public int boidGroupSize = 5;

    [Header("Algae Settings")]
    public GameObject algaePrefab;
    public int maxAlgaeCount = 7;
    public float algaeY = 0f;              // vị trí Y đáy
    public float algaeXSpacing = 2f;       // khoảng cách tối thiểu giữa các cây tảo
    public Vector2 algaeXRange = new Vector2(1f, 19f);
    public float algaeSpawnInterval = 2f;  // spawn mỗi X giây

    private float timer = 0f;
    private float algaeTimer = 0f;

    [Header("Medium Fish Settings")]
    public GameObject mediumFishPrefab;
    public float mediumFishSpawnInterval = 8f;  // spawn mỗi X giây
    private float mediumFishTimer = 0f;

    void Update()
    {
        // spawn cá
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnFish();
        }

        // spawn tảo
        algaeTimer += Time.deltaTime;
        if (algaeTimer >= algaeSpawnInterval)
        {
            algaeTimer = 0f;
            TrySpawnAlgae();
        }
    }

    void SpawnFish()
    {
        // random spawn side
        bool spawnLeft = Random.value < 0.5f;
        float xPos = spawnLeft ? spawnLeftX : spawnRightX;
        float yPos = Random.Range(-spawnYRange, spawnYRange);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        int dir = spawnLeft ? 1 : -1;

        // chọn cá theo tỉ lệ
        float r = Random.value;
        if (r < straightChance)
        {
            var go = Instantiate(fishStraightPrefab, spawnPos, Quaternion.identity);
            SetupDirection(go, dir);
        }
        else if (r < straightChance + waveChance)
        {
            var go = Instantiate(fishWavePrefab, spawnPos, Quaternion.identity);
            SetupDirection(go, dir);
        }
        else
        {
            for (int i = 0; i < boidGroupSize; i++)
            {
                Vector3 offset = new Vector3(i * 0.3f * dir, Random.Range(-0.5f, 0.5f), 0);
                var go = Instantiate(boidPrefab, spawnPos + offset, Quaternion.identity);

                Boid boid = go.GetComponent<Boid>();
                if (boid != null) boid.SetDirection(dir);

                SetupDirection(go, dir);
            }
        }
    }


    void SetupDirection(GameObject go, int dir)
    {
        // FishStraight
        FishStraight straight = go.GetComponent<FishStraight>();
        if (straight != null)
            straight.direction = Vector2.right * dir;

        // FishWave
        FishWave wave = go.GetComponent<FishWave>();
        if (wave != null)
            wave.speed *= dir;

        // Boid
        Boid boid = go.GetComponent<Boid>();
        if (boid != null)
            boid.velocity = new Vector2(dir, 0) * boid.speed;

        // Quay mặt (flip scale X)
        go.transform.localScale = new Vector3(dir * Mathf.Abs(go.transform.localScale.x),
                                              go.transform.localScale.y,
                                              go.transform.localScale.z);
    }

    void TrySpawnAlgae()
    {
        // kiểm tra giới hạn số lượng
        Algae[] allAlgae = FindObjectsOfType<Algae>();
        if (allAlgae.Length >= maxAlgaeCount) return;

        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float xPos = Random.Range(algaeXRange.x, algaeXRange.y);

            bool tooClose = false;
            foreach (Algae a in allAlgae)
            {
                if (Mathf.Abs(a.transform.position.x - xPos) < algaeXSpacing)
                {
                    tooClose = true;
                    break;
                }
            }

            if (!tooClose)
            {
                // spawn tảo ở vị trí hợp lệ
                Vector3 pos = new Vector3(xPos, algaeY, 0);
                Instantiate(algaePrefab, pos, Quaternion.identity);
                return;
            }
        }
    }
}
