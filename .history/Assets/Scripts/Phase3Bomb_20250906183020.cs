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
        // NOTE: không return sớm bằng 'consumed' để bomb luôn tồn tại và xử lý mọi va chạm
        var fish = other.GetComponentInParent<Fish>();
        if (fish != null)
        {
            // Player chết ngay
            if (fish.isPlayer)
            {
                Debug.Log($"[Phase3Bomb] Player hit by bomb ({name}) -> Die()");
                fish.Die();
                return; // xong, bomb vẫn tồn tại
            }

            // Thử detect Boss trực tiếp (không dựa vào fish.isBoss flag)
            var boss = other.GetComponentInParent<Boss>();
            if (boss != null)
            {
                // Nếu boss đang stun/invulnerable thì bỏ qua (boss đã có cơ chế isInvulnerable)
                if (boss.IsStunned)
                {
                    Debug.Log($"[Phase3Bomb] Boss hit but already stunned/invulnerable -> ignored");
                    return;
                }

                Debug.Log($"[Phase3Bomb] Boss hit by bomb ({name}) -> TakeDamage + SpawnMeat");
                // Boss nhận damage + stun -> boss.TakeDamage tự set invulnerable
                boss.TakeDamage(boss.phase3BombHitDamage, boss.phase3BombStunDuration);

                // Spawn meat và bắt đầu quá trình ăn lại
                if (boss.currentState is BossPhase3State phase3)
                {
                    phase3.SpawnMeatOnBombHit(boss);
                    // gọi coroutine quản lý ăn thịt / nghỉ / quay lại phase
                    boss.StartCoroutine(phase3.Phase3AfterStun(boss));
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

    /// <summary>
    /// Đánh dấu bomb đã xử lý (ví dụ khi BossPhase3State xử lý boss đâm bomb)
    /// </summary>

}
