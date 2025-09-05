using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Phase3Bomb : MonoBehaviour
{
    [HideInInspector] public float bombRadius = 1f;
    private bool consumed = false;

    void Start()
    {
        // Nếu chưa được ApplyRadius() từ BossPhase3State thì vẫn dùng bombRadius mặc định
        ApplyRadius(bombRadius);
    }

    /// <summary>
    /// Gọi hàm này ngay sau khi Instantiate để cập nhật collider + scale đúng theo bán kính
    /// </summary>
    public void ApplyRadius(float r)
    {
        bombRadius = r;

        // scale sprite để khớp bán kính
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.bounds.size.x > 0.01f)
        {
            float spriteSize = sr.bounds.size.x;
            float scale = (bombRadius * 2f) / spriteSize;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // set collider
        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.isTrigger = false; // bomb là "tường cứng"
            circle.radius = bombRadius;
        }
    }

    private void HandleHit(GameObject other)
    {
        if (consumed) return;

        var player = other.GetComponentInParent<Fish>();
        if (player != null && player.isPlayer)
        {
            Debug.Log("Bomb: Player die!");
            player.Die();
            consumed = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Bomb collision với: {collision.gameObject.name}");
        HandleHit(collision.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Bomb trigger với: {other.gameObject.name}");
        HandleHit(other.gameObject);
    }

    /// <summary>
    /// Đánh dấu bomb đã xử lý (ví dụ khi BossPhase3State xử lý boss đâm bomb)
    /// </summary>
    public void MarkConsumed()
    {
        consumed = true;
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }
}
