using UnityEngine;

/// <summary>
/// Fish cơ bản (player & enemy & algae-segment cũng dùng Fish để dễ xử lý ăn).
/// LƯU Ý: không ép thay đổi collider/body type trong Awake để tránh phá physics của segment.
/// Các prefab cá/player nên cấu hình Rigidbody2D/Collider2D đúng trong Inspector.
/// </summary>
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

    [Header("Hunger Value")]
    [Tooltip("Độ no mà con cá này cung cấp cho cá người chơi khi bị ăn")]
    public float hungerValue = 10f;

    // ---- internal ----
    protected Rigidbody2D rb;
    protected Collider2D col;
    private MapManager mapManager;
    private float spawnTime;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // KHÔNG tự ép collider.isTrigger hay rb.bodyType mặc định ở đây
        // Giữ nguyên cấu hình prefab để phân biệt fish (trigger kinematic) và algae-segment (dynamic non-trigger)

        ApplyScale();

        mapManager = FindObjectOfType<MapManager>();
        if (mapManager == null)
            Debug.LogWarning("Fish: Không tìm thấy MapManager trong scene!");

        spawnTime = Time.time;
    }

    protected virtual void Update()
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

    protected void ApplyScale()
    {
        transform.localScale = Vector3.one * size;
    }

    // NOTE: predator calls this when trigger enters.
    // We keep logic: if this.size > other.size -> Eat(other)
    protected virtual void OnTriggerEnter2D(Collider2D other)
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

    // central Eat function: before destroying prey, check if prey is an AlgaeSegment
    protected virtual void Eat(Fish prey)
    {
        if (prey == null) return;

        // If prey is an AlgaeSegment, ask its chain if allowed to eat
        AlgaeSegment seg = prey.GetComponent<AlgaeSegment>();
        if (seg != null && seg.chain != null)
        {
            bool allowed = seg.chain.TryEatSegment(prey.gameObject, eater: this);
            if (!allowed)
            {
                // not allowed to eat (out of order) -> do nothing
                return;
            }
            // else chain already removed the segment (or chain will allow and let predator handle vfx/score)
        }
        else
        {
            // normal prey (not algae) -> destroy normally below
            if (eatVfxPrefab != null)
                Instantiate(eatVfxPrefab, prey.transform.position, Quaternion.identity);

            if (eatSound != null)
                AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);

            if (isPlayer && GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(prey.scoreValue);

                // >>> cộng hunger khi player ăn
                if (PlayerHunger.Instance != null)
                {
                    PlayerHunger.Instance.GainHunger(prey.hungerValue);
                }
            }

            Destroy(prey.gameObject);
        }

        // If prey was algae and chain allowed eating, chain.TryEatSegment has taken care of destruction
        // But we still play VFX / sound / score if eater is player
        if (seg != null && seg.chain != null)
        {
            if (eatVfxPrefab != null)
                Instantiate(eatVfxPrefab, seg.transform.position, Quaternion.identity);

            if (eatSound != null)
                AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);

            if (isPlayer && GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(prey.scoreValue);

                // >>> cộng hunger cả khi ăn rong
                if (PlayerHunger.Instance != null)
                {
                    PlayerHunger.Instance.GainHunger(prey.hungerValue);
                }
            }
        }
    }


    public void SetSize(float newSize)
    {
        size = newSize;
        ApplyScale();
    }
}
