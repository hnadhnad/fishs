using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Fish : MonoBehaviour
{
    [Header("Size & Growth")]
    public float size = 1f;
    public float minSize = 0.4f;
    public float maxSize = 6f;
    public float eatThreshold = 1.2f;
    public float growthMultiplier = 0.6f;

    [Header("Feedback")]
    public AudioClip eatSound;
    public GameObject eatVfxPrefab;

    [Header("Bounds Settings")]
    [Tooltip("Khoảng đệm ngoài giới hạn map trước khi cá bị huỷ")]
    public float despawnMargin = 1f;
    [Tooltip("Thời gian ân hạn sau khi spawn (chỉ áp dụng cho Destroy, vẫn bị ăn bình thường)")]
    public float spawnGraceTime = 1.5f;

    Rigidbody2D rb;
    Collider2D col;
    MapManager mapManager;
    float spawnTime;

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

        spawnTime = Time.time; // đánh dấu thời điểm spawn
    }

    void Update()
    {
        // Chỉ bỏ qua check Destroy trong khoảng spawnGraceTime
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

    public void SetSize(float newSize)
    {
        size = Mathf.Clamp(newSize, minSize, maxSize);
        ApplyScale();
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

        if (this.size >= otherFish.size * eatThreshold)
        {
            Eat(otherFish);
            return;
        }

        if (otherFish.size >= this.size * otherFish.eatThreshold)
        {
            return;
        }

        Vector2 dir = (transform.position - other.transform.position).normalized;
        if (rb != null) rb.AddForce(dir * 20f);
    }

    void Eat(Fish prey)
    {
        if (prey == null) return;

        if (eatVfxPrefab != null)
            Instantiate(eatVfxPrefab, prey.transform.position, Quaternion.identity);

        if (eatSound != null)
            AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);

        float gained = prey.size * growthMultiplier;
        SetSize(this.size + gained);

        Destroy(prey.gameObject);
    }
}
