using UnityEngine;

/// <summary>
/// Bomb kiểu telegraph: cấu hình bằng Configure(...).
/// Sau "fallDuration" (thời gian đếm/telegraph) sẽ nổ tại vị trí hiện tại:
/// - Player trong vùng: gọi Die() (tuỳ hệ thống Fish của bạn).
/// - Boss trong vùng: boss.TakeDamage(damage) + boss.Stun(stunDuration) và **chỉ khi đó** spawn thịt.
/// Thịt rơi thành vòng, cách tâm boss "meatSpawnOffset", tản ra với "meatScatterSpeed".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FallingBomb : MonoBehaviour
{
    [Header("Runtime Config")]
    public float fallDuration = 0.9f;      // thời gian đếm trước khi nổ
    public float explodeRadius = 1.6f;     // bán kính nổ
    public float damage = 120f;            // damage vào boss
    public GameObject meatPrefab = null;   // prefab thịt
    public int meatCount = 4;              // số miếng thịt
    public float meatSpawnOffset = 0.8f;   // khoảng cách vòng thịt tới tâm boss
    public float meatScatterSpeed = 3f;    // vận tốc đẩy miếng thịt ra xa
    public float bossStunDuration = 1.5f;  // thời gian choáng boss khi dính nổ

    private bool _configured = false;
    private float _timer = 0f;

    public void Configure(
        float fallDuration,
        float explodeRadius,
        float damage,
        GameObject meatPrefab,
        int meatCount,
        float meatSpawnOffset,
        float meatScatterSpeed,
        float bossStunDuration)
    {
        this.fallDuration     = Mathf.Max(0.01f, fallDuration);
        this.explodeRadius    = Mathf.Max(0.01f, explodeRadius);
        this.damage           = Mathf.Max(0f, damage);
        this.meatPrefab       = meatPrefab;
        this.meatCount        = Mathf.Max(0, meatCount);
        this.meatSpawnOffset  = Mathf.Max(0f, meatSpawnOffset);
        this.meatScatterSpeed = Mathf.Max(0f, meatScatterSpeed);
        this.bossStunDuration = Mathf.Max(0f, bossStunDuration);

        _timer = 0f;
        _configured = true;

        if (TryGetComponent<Collider2D>(out var col))
            col.isTrigger = true; // telegraph/trigger
    }

    void Start()
    {
        if (!_configured) _configured = true; // cho phép test nhanh trên scene
    }

    void Update()
    {
        if (!_configured) return;

        _timer += Time.deltaTime;
        if (_timer >= fallDuration)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        bool bossDamaged = false;
        Vector3 bossPosAtHit = Vector3.zero;

        foreach (var h in hits)
        {
            if (h == null) continue;

            // Player: chết khi đứng trong vùng
            var f = h.GetComponent<Fish>();
            if (f != null && f.isPlayer)
            {
                f.Die();
            }

            // Boss: nhận damage + stun (chỉ 1 lần)
            var boss = h.GetComponent<Boss>();
            if (!bossDamaged && boss != null)
            {
                boss.TakeDamage(damage);
                boss.Stun(bossStunDuration);
                bossDamaged = true;
                bossPosAtHit = boss.transform.position;
            }
        }

        // Spawn thịt CHỈ khi bomb trúng boss
        if (bossDamaged && meatPrefab != null && meatCount > 0)
        {
            float healPerPiece = damage / Mathf.Max(1, meatCount);
            float step = 360f / meatCount;

            for (int i = 0; i < meatCount; i++)
            {
                float ang = step * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f);
                Vector3 spawnPos = bossPosAtHit + dir * meatSpawnOffset;

                GameObject piece = Instantiate(meatPrefab, spawnPos, Quaternion.identity);

                // Bắt buộc có MeatPiece để boss ăn hồi máu
                if (!piece.TryGetComponent<MeatPiece>(out var mp))
                    mp = piece.AddComponent<MeatPiece>();

                mp.SetHealAmount(healPerPiece);

                // đẩy ra xa nhẹ (nếu có Rigidbody2D)
                if (piece.TryGetComponent<Rigidbody2D>(out var rb))
                    rb.velocity = dir * meatScatterSpeed;
            }
        }

        // (có thể play VFX/SFX tại đây)

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
