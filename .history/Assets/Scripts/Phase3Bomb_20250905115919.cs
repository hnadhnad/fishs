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

        // lấy sprite gốc
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            // chiều rộng sprite ở đơn vị world
            float spriteWorldSize = sr.sprite.bounds.size.x;
            
            // cần scale sao cho bán kính thật = bombRadius
            float targetDiameter = bombRadius * 2f;
            float scale = targetDiameter / spriteWorldSize;

            transform.localScale = new Vector3(scale, scale, 1f);
        }

        var circle = GetComponent<CircleCollider2D>();
        if (circle != null)
        {
            circle.isTrigger = false;

            // để radius mặc định = 0.5 (unit circle), không set = bombRadius nữa
            circle.radius = 0.5f; 
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
