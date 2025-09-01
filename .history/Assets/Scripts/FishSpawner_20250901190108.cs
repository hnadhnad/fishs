using UnityEngine;

/// <summary>
/// Spawner sinh cá (thẳng, lượn sóng, đàn boid), cá đặc biệt (Medium, Big) và tảo.
/// Mỗi prefab cá tự đặt size sẵn trong Inspector, không random size nữa.
/// </summary>
public class FishSpawner : MonoBehaviour
{
    [Header("Spawner Toggle")]
    [SerializeField]public bool enableSpawning = true; 
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
    public GameObject algaePrefab;    // prefab AlgaeChain
    public Vector2[] algaePositions;  // danh sách vị trí spawn tảo cố định

    private float timer = 0f;

    [Header("Medium Fish Settings")]
    public GameObject mediumFishPrefab;
    public float mediumFishSpawnInterval = 8f;  // spawn mỗi X giây
    private float mediumFishTimer = 0f;

    [Header("Big Fish Settings")]
    public GameObject bigFishPrefab;
    public float bigFishSpawnInterval = 10f; // khoảng cách thời gian spawn
    private float bigFishTimer = 0f;

    void Start()
    {
        // spawn algae khi bắt đầu map
        SpawnAlgae();
    }

    void Update()
    {
        if (!enableSpawning) return;  
        // spawn cá thường
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            SpawnFish();
        }

        // spawn MediumFish riêng
        mediumFishTimer += Time.deltaTime;
        if (mediumFishTimer >= mediumFishSpawnInterval)
        {
            mediumFishTimer = 0f;
            SpawnMediumFish();
        }

        // spawn BigFish
        bigFishTimer += Time.deltaTime;
        if (bigFishTimer >= bigFishSpawnInterval)
        {
            bigFishTimer = 0f;
            SpawnBigFish();
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
            wave.direction = dir;   // chỉ set direction, đừng nhân speed

        // Boid
        Boid boid = go.GetComponent<Boid>();
        if (boid != null)
            boid.velocity = new Vector2(dir, 0) * boid.speed;

        // Quay mặt (flip scale X)
        go.transform.localScale = new Vector3(dir * Mathf.Abs(go.transform.localScale.x),
                                            go.transform.localScale.y,
                                            go.transform.localScale.z);
    }

    void SpawnMediumFish()
    {
        bool spawnLeft = Random.value < 0.5f;
        float xPos = spawnLeft ? spawnLeftX : spawnRightX;
        float yPos = Random.Range(-spawnYRange, spawnYRange);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        int dir = spawnLeft ? 1 : -1;

        var go = Instantiate(mediumFishPrefab, spawnPos, Quaternion.identity);

        MediumFish mf = go.GetComponent<MediumFish>();
        if (mf != null) mf.direction = dir;

        go.transform.localScale = new Vector3(dir * Mathf.Abs(go.transform.localScale.x),
                                            go.transform.localScale.y,
                                            go.transform.localScale.z);
    }

    void SpawnBigFish()
    {
        bool spawnLeft = Random.value < 0.5f;
        float xPos = spawnLeft ? spawnLeftX : spawnRightX;
        float yPos = Random.Range(-spawnYRange, spawnYRange);
        Vector3 spawnPos = new Vector3(xPos, yPos, 0);

        int dir = spawnLeft ? 1 : -1;

        var go = Instantiate(bigFishPrefab, spawnPos, Quaternion.identity);

        BigFish bf = go.GetComponent<BigFish>();
        if (bf != null) bf.direction = dir;

        go.transform.localScale = new Vector3(dir * Mathf.Abs(go.transform.localScale.x),
                                            go.transform.localScale.y,
                                            go.transform.localScale.z);
    }

    void SpawnAlgae()
    {
        if (algaePrefab == null) return;

        if (algaePositions != null && algaePositions.Length > 0)
        {
            foreach (Vector2 pos in algaePositions)
            {
                Instantiate(algaePrefab, pos, Quaternion.identity);
            }
        }
    }
}
