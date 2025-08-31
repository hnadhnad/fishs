using UnityEngine;

/// <summary>
/// Fish: mỗi cá có size (đặt sẵn trong prefab).
/// Khi va chạm: cá size lớn hơn ăn cá size nhỏ hơn.
/// Nếu cá là player → cộng điểm cho GameManager.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Fish : MonoBehaviour
{
    [Header("Settings")]
    public bool isPlayer = false;   // true nếu là cá người chơi
    public float size = 1f;         // scale hiển thị & tiêu chí ăn
    public int scoreValue = 10;     // điểm thưởng khi player ăn con khác

    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();

        // Collider phải là trigger
        if (col != null) col.isTrigger = true;

        // Rigidbody2D để trigger hoạt động
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Set scale theo size
        transform.localScale = Vector3.one * size;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other == col) return;

        Fish otherFish = other.GetComponent<Fish>();
        if (otherFish == null || otherFish == this) return;

        // Nếu cùng size thì không ai ăn ai
        if (Mathf.Approximately(this.size, otherFish.size)) return;

        // Cá lớn hơn ăn cá nhỏ hơn
        if (this.size > otherFish.size)
        {
            Eat(otherFish);
        }
    }

    private void Eat(Fish prey)
    {
        if (prey == null) return;

        // Nếu là player → cộng điểm
        if (isPlayer && GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(prey.scoreValue);
        }

        Destroy(prey.gameObject);
    }
}
