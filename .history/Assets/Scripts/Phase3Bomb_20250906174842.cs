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

        var fish = other.GetComponentInParent<Fish>();
        if (fish != null)
        {
            if (fish.isPlayer)
            {
                Debug.Log("Bomb: Player die!");
                fish.Die();
                consumed = true;
            }
            else if (fish.isBoss)
            {
                Debug.Log("Bomb: Boss va chạm → stun + spawn thịt");

                var boss = fish.GetComponent<Boss>();
                if (boss != null)
                {
                    // spawn thịt
                    if (boss.currentState is BossPhase3State phase3)
                    {
                        phase3.SpawnMeatOnBombHit(boss);

                        // 🔥 Gọi Phase3AfterStun để boss ăn thịt sau khi hết choáng
                        boss.StartCoroutine(phase3.Phase3AfterStun(boss));
                    }

                    // stun boss
                    boss.Stun(boss.phase3BombStunDuration);
                }
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
