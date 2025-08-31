using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Fish : MonoBehaviour
{
    [Header("Fish Type")]
    public bool isPlayer = false;

    [Header("Score")]
    public int scoreValue = 10;

    [Header("Size")]
    [Tooltip("Kích thước cố định (set trong prefab)")]
    public float size = 1f;

    [Header("Feedback")]
    public AudioClip eatSound;
    public GameObject eatVfxPrefab;

    [Header("Bounds Settings")]
    [Tooltip("Khoảng đệm ngoài giới hạn map trước khi cá bị huỷ")]
    public float despawnMargin = 1f;
    [Tooltip("Thời gian ân hạn sau khi spawn (chỉ áp dụng cho Destroy, vẫn bị ăn bình thường)")]
    public float spawnGraceTime = 1.5f;

    private Rigidbody2D rb;
    private Collider2D col;
    private MapManager mapManager;
    private float spawnTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (col != null) col.isTrigger = true;
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        ApplyScale();

        mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
            Debug.LogError("Không tìm thấy MapManager trong scene!");

        spawnTime = Time.time;
    }

    void Update()
    {
        // chỉ kiểm tra despawn sau khoảng spawnGraceTime
        if (Time.time - spawnTime < spawnGraceTime) return;

        if (mapManager != null)
        {
            Vector2 pos = transform.position;
            if (pos.x < mapManager.bottomLeft.x - despawnMargin ||
                pos.x > mapManager.topRight.x + despawnMargin ||
                pos.y < mapManager.bottomLeft.y - despawnMargin ||
                pos.y > mapManager.topRight.y + despawnMargin)
            {
                Destroy(gameObject);
            }
        }
    }

    void ApplyScale()
    {
        transform.localScale = Vector3.one * size;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || other.gameObject == this.gameObject) return;

        Fish otherFish = other.GetComponent<Fish>();
        if (otherFish == null) return;

        // Nếu cùng size thì không ai ăn ai
        if (Mathf.Approximately(this.size, otherFish.size)) return;

        if (this.size > otherFish.size)
        {
            Eat(otherFish);
        }
    }

    void Eat(Fish prey)
    {
        if (prey == null) return;

        if (eatVfxPrefab != null)
            Instantiate(eatVfxPrefab, prey.transform.position, Quaternion.identity);

        if (eatSound != null)
            AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);

        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(prey.scoreValue);
        }

        Destroy(prey.gameObject);
    }
    public void SetSize(float newSize)
    {
        size = newSize;
        ApplyScale();
    }

}
