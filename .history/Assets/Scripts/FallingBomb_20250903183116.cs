using System.Collections;
using UnityEngine;

/// <summary>
/// Bomb: sau fallDuration gọi Explode() — không phụ thuộc animation.
/// Explode => OverlapCircleAll => áp dụng hiệu ứng:
///  - Player (tag "Player" hoặc component Player) => chết (call Die() nếu có)
///  - Boss (component Boss) => TakeDamage(damage)
///  - spawn meat pieces (meatPrefab) with healAmount = damage / meatCount
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class FallingBomb : MonoBehaviour
{
    [Header("Config (can be set by Configure)")]
    public float fallDuration = 0.9f;
    public float explodeRadius = 1.6f;
    public float damage = 120f;
    public GameObject meatPrefab = null;
    public int meatCount = 4;

    bool _configured = false;
    float _timer = 0f;

    /// <summary>
    /// Configure bomb at runtime (preferred)
    /// </summary>
    public void Configure(float fallDuration, float explodeRadius, float damage, GameObject meatPrefab, int meatCount)
    {
        this.fallDuration = Mathf.Max(0.01f, fallDuration);
        this.explodeRadius = Mathf.Max(0.01f, explodeRadius);
        this.damage = damage;
        this.meatPrefab = meatPrefab;
        this.meatCount = Mathf.Max(0, meatCount);
        _timer = 0f;
        _configured = true;

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void Start()
    {
        // if not configured externally, just mark configured so Update runs normally
        if (!_configured) _configured = true;
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

    void Explode()
    {
        // 1) Overlap to find anything in radius
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRadius);
        bool bossDamaged = false;

        foreach (var h in hits)
        {
            if (h == null) continue;

            // Player: if player exists and in area -> kill
            // We try to find a Fish that isPlayer
            Fish f = h.GetComponent<Fish>();
            if (f != null && f.isPlayer)
            {
                // kill player via Die() if available
                f.Die();
            }

            // Boss: damage
            Boss boss = h.GetComponent<Boss>();
            if (boss != null && !bossDamaged)
            {
                // Apply damage once per explosion to boss
                if (boss.TryGetComponent(out Boss bComp))
                {
                    // prefer TakeDamage method if present
                    try
                    {
                        bComp.TakeDamage(damage);
                    }
                    catch
                    {
                        // fallback directly
                        bComp.currentHealth = Mathf.Max(0f, bComp.currentHealth - damage);
                        if (bComp.currentHealth <= 0f)
                        {
                            // attempt Die if exists
                            bComp.SendMessage("Die", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                }
                bossDamaged = true;
            }
        }

        // 2) Spawn meat pieces: each piece healAmount = damage / meatCount
        if (meatPrefab != null && meatCount > 0)
        {
            float healPerPiece = damage / Mathf.Max(1, meatCount);

            // spawn pieces evenly around circle (or 4 directions if meatCount =4)
            float angleStep = 360f / meatCount;
            for (int i = 0; i < meatCount; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
                Vector3 spawnPos = transform.position + dir * 0.3f;

                GameObject m = Instantiate(meatPrefab, spawnPos, Quaternion.identity);

                // add/ensure MeatPiece script to handle boss-heal
                var mp = m.GetComponent<MeatPiece>();
                if (mp == null) mp = m.AddComponent<MeatPiece>();

                mp.SetHealAmount(healPerPiece);
                // optionally give initial velocity away from center so pieces scatter (if they have Rigidbody2D)
                if (m.TryGetComponent<Rigidbody2D>(out var rb))
                {
                    rb.velocity = dir * 2.5f;
                }
            }
        }

        // optionally spawn VFX/SFX here

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRadius);
    }
}
