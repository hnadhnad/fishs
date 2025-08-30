using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Fish : MonoBehaviour
{
    [Header("Size & Growth")]
    public float size = 1f;                 // "kích cỡ" logic (1 = mặc định)
    public float minSize = 0.4f;
    public float maxSize = 6f;
    [Tooltip("Tỉ lệ cần lớn hơn để có thể nuốt (ví dụ 1.2 = cần lớn hơn 120%)")]
    public float eatThreshold = 1.2f;
    [Tooltip("Tăng kích thước sau khi nuốt: growth += prey.size * growthMultiplier")]
    public float growthMultiplier = 0.6f;

    [Header("Feedback")]
    public AudioClip eatSound;
    public GameObject eatVfxPrefab;

    [Header("World Bounds")]
    public static Rect worldBounds = new Rect(-15, -6, 30, 12);
    // left=-15, bottom=-6, width=30, height=12  => right=15, top=6
    // Có thể chỉnh trong code: Fish.worldBounds = new Rect(...);

    // Internal
    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // đảm bảo collider là trigger để OnTriggerEnter2D hoạt động
        if (col != null) col.isTrigger = true;

        // chuyển Rigidbody sang kinematic (di chuyển bằng code)
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

        // áp scale ban đầu theo size
        ApplyScale();
    }

    void Update()
    {
        // nếu ra ngoài worldBounds thì destroy
        if (!worldBounds.Contains(transform.position))
        {
            Destroy(gameObject);
        }
    }

    public void SetSize(float newSize)
    {
        size = Mathf.Clamp(newSize, minSize, maxSize);
        ApplyScale();
    }

    void ApplyScale()
    {
        // Tỷ lệ localScale = size (càng lớn -> cá càng to)
        transform.localScale = Vector3.one * size;
    }

    // Mọi cá khi chạm nhau sẽ kiểm tra ăn/nếu đủ điều kiện
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;
        if (other.gameObject == this.gameObject) return;

        Fish otherFish = other.GetComponent<Fish>();
        if (otherFish == null) return;

        // nếu mình lớn hơn và đạt ngưỡng -> ăn
        if (this.size >= otherFish.size * eatThreshold)
        {
            Eat(otherFish);
            return;
        }

        // nếu bị cá khác lớn hơn ăn thì để con khác xử lý (ngược lại)
        if (otherFish.size >= this.size * otherFish.eatThreshold)
        {
            return;
        }

        // nếu kích thước gần bằng nhau => đẩy nhẹ ra để tránh dính
        Vector2 dir = (transform.position - other.transform.position).normalized;
        if (rb != null) rb.AddForce(dir * 20f);
    }

    void Eat(Fish prey)
    {
        if (prey == null) return;

        // play vfx & sound
        if (eatVfxPrefab != null)
        {
            Instantiate(eatVfxPrefab, prey.transform.position, Quaternion.identity);
        }

        if (eatSound != null)
        {
            AudioSource.PlayClipAtPoint(eatSound, Camera.main.transform.position);
        }

        // tăng size
        float gained = prey.size * growthMultiplier;
        SetSize(this.size + gained);

        // huỷ cá bị ăn
        Destroy(prey.gameObject);
    }
}
