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
        // tránh xử lý nhiều lần bởi cùng 1 bomb (OnCollision + OnTrigger hoặc nhiều callbacks)
        if (consumed) return;

        // NOTE: dùng GetComponentInParent để hỗ trợ nested colliders
        var fish = other.GetComponentInParent<Fish>();
        if (fish != null)
        {
            // Player chết ngay
            if (fish.isPlayer)
            {
                Debug.Log($"[Phase3Bomb] Player hit by bomb ({name}) -> Die()");
                fish.Die();

                // không mark consumed nếu muốn bomb vẫn tồn tại; nhưng để tránh double-callback ta vẫn disable tiếp
                consumed = true;
                var col = GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                return;
            }

            // Thử detect Boss trực tiếp (không dựa vào fish.isBoss flag)
            var boss = other.GetComponentInParent<Boss>();
            if (boss != null)
            {
                // nếu bomb đã được xử lý (consumed) -> return
                // (đã xử lý phía trên) nhưng vẫn kiểm tra boss trạng thái
                if (boss.IsStunned)
                {
                    Debug.Log($"[Phase3Bomb] Boss hit but already stunned/invulnerable -> ignored");
                    // mark consumed để tránh callback kép với cùng bomb
                    consumed = true;
                    var col = GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;
                    return;
                }

                Debug.Log($"[Phase3Bomb] Boss hit by bomb ({name}) -> TakeDamage + SpawnMeat (requested)");

                // mark consumed ngay để tránh double handling từ cùng bomb (OnTrigger + OnCollision)
                consumed = true;
                var collider = GetComponent<Collider2D>();
                if (collider != null) collider.enabled = false;

                // Boss nhận damage + stun -> boss.TakeDamage tự set invulnerable
                boss.TakeDamage(boss.phase3BombHitDamage, boss.phase3BombStunDuration);

                // Thay vì spawn meat trực tiếp ở đây (có thể gây nhiều lần),
                // gọi API trên state để debounced + đảm bảo Phase3AfterStun chỉ được start 1 lần
                if (boss.currentState is BossPhase3State phase3)
                {
                    phase3.TryHandleBombHit(boss);
                }

                return;
            }
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
}
